
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{

	// As usual,
	// The currency markets can do ANYTHING at ANYTIME.
	// No warranty is provided for this product and no suitability is implied for any use.
	// There are no protections included in this code that can limit the outcomes from its use.
	// The user is solely responsible for determining when, where and how to use this code.
	// By using this product, the user accepts full liability related to the use of this porduct and the outcome for doing so.

	/// <summary>  
	///  The ALMA class formulates the ALMA Indicator on an Alveo currency chart.  
	///    
	///  The period parameter set the strength of the filtering by the ALMA.  
	///  The slopeThreshold parameter specifies at what slope Uptrend and Downtrend are determined.  
	///  The slopeThreshold parameter allows the user to decide which slope value signals a strong Uptrend or Downtrend.  
	///  
	///  This Indicator calculates three lines on the chart:.  
	///    * Uptrend in Blue
	///    * Downtrend in Red
	///    * Consolidation in Green
	/// 
	/// </summary>  

	[Serializable]
	[Description("Alveo ALMA Indicator")]
	public class TREND_CHANNEL : IndicatorBase
	{
		#region Properties

		// User settable Properties for this Alveo Indicator
		/// <param name="period">Sets the strength of the filtering.</param>
		[Category("Settings")]
		[DisplayName("Smoother Period. ")]
		[Description("Sets the strength of the smoother filter. [ex: 20]")]
		public int IndPeriod { get; set; }

		[Category("Settings")]
		[DisplayName("Cutoff Period. ")]
		[Description("Sets the cutoff period for the Decycler (centre line). [ex: 200]")]
		public int CutoffPeriod { get; set; }

		[Category("Settings")]
		[DisplayName("Band offset Factor. ")]
		[Description("Factor used to multiply the average distance from the centreline. [ex: 2]")]
		public double Offset_Factor { get; set; }

		[Category("Settings")]
		[DisplayName("Min Band Offset (pips). ")]
		[Description("Minimum band offset pips. [ex: 40]")]
		public double Min_Offset { get; set; }

		/// <param name="slopeThreshold">Specifies at what slope Uptrend and Downtrend are determined.</param>
		[Category("Settings")]
		[DisplayName("Slope Trheshold * 1e-6. ")]
		[Description("Specifies at what slope Uptrend and Downtrend are determined. [ex: 10]")]
		public int SlopeThreshold { get; set; }

		#endregion

		//Buffers for Indicator
		Array<double> UpTrend;          // upwards trend
		Array<double> DownTrend;        // downwards trend
		Array<double> Consolidation;    // in between UpTrend and DownTrend
		Array<double> Upper;
		Array<double> Lower;
		//Array<double> AbsDifference;

		Bar b;              // holds latest chart Bar data from Alveo
		Bar prevBar;        // holds latest chart Bar data from Alveo
		int counted_bars;   // amount of bars of bars on chart already processed by the indicator.
		int e;              // number of bars for the indicator to calculate
		double thePrice;    // holds the currency pair price for the NDI calculation. In units of the bas currency.
		bool firstrun;      // firstrun = true on first execution of the Start function. False otherwise.
		double bandOffset;
		double prev_max_AbsDifference;
		double max_AbsDifference;
		double Deviation;
		double DeviationAbs;
		double MeanDeviation;

		internal Decycler_obj priceDecycle;
		internal SUPERSMOOTHER_3Pole_obj priceDecycleSmooth;
		internal SUPERSMOOTHER_3Pole_obj priceSmooth;
		internal ALMAobj deviationMean;

		const string dataFileDir = "C:\\temp\\ALMA\\";

		/// </summary>  
		public TREND_CHANNEL()
		{
			try
			{
				// Basic indicator initialization. Don't use this constructor to calculate values

				indicator_buffers = 5;              // 3 lines on Alveo chart require 3 buffers
				indicator_chart_window = true;
				firstrun = true;                    // initially set true for first run
				prevBar = null;

				IndPeriod = 20;                     // Initial value for DTEMAv2 period
				SlopeThreshold = 10;                // Initial value for slopeThreshold
				Offset_Factor = 2;
				Min_Offset = 40;
				bandOffset = 0;
				Deviation = 0;
				MeanDeviation = 0;
				prev_max_AbsDifference = 0.0;
				max_AbsDifference = 0.0;
				CutoffPeriod = 200;

				Color transparentWhite = Color.FromArgb(80, 255, 255, 255); //55 / 255 = 50 % of transparency

				indicator_width1 = 1;               // width of line 1 on the chart
				indicator_width2 = 1;
				indicator_width3 = 1;
				indicator_width4 = 1;
				indicator_width5 = 1;
				indicator_color1 = transparentWhite;     // line colors
				indicator_color2 = transparentWhite;
				indicator_color3 = transparentWhite;
				indicator_color4 = transparentWhite;
				indicator_color5 = transparentWhite;

				UpTrend = new Array<double>();      // 3 data buffers for Alveo Indicator
				DownTrend = new Array<double>();
				Consolidation = new Array<double>();
				Upper = new Array<double>();
				Lower = new Array<double>();

				copyright = "";
				link = "";
			}
			catch (Exception ex)  // Print don't function at this point in Alveo
			{
				Print("SUPERSMOOTHER_3Pole: Exception: " + ex.Message);
				Print("SUPERSMOOTHER_3Pole: " + ex.StackTrace);
			}
		}

		//+------------------------------------------------------------------+");
		//| Custom indicator initialization function                         |");
		//| Called by Alveo to initialize the ALMA Indicator at startup.  |");
		//+------------------------------------------------------------------+");
		protected override int Init()
		{
			try
			{
				// ENTER YOUR CODE HERE
				IndicatorBuffers(indicator_buffers);        // Allocates memory for buffers used for custom indicator calculations.
				SetIndexBuffer(0, UpTrend);                 // binds a specified indicator buffer with one-dimensional dynamic array of the type double.
				SetIndexArrow(0, 159);                      // Sets an arrow symbol for indicators line of the DRAW_ARROW type. 159=dot.
				SetIndexBuffer(1, DownTrend);               // repeat for each buffer
				SetIndexArrow(1, 159);
				SetIndexBuffer(2, Consolidation);
				SetIndexArrow(2, 159);
				SetIndexBuffer(3, Upper);
				SetIndexBuffer(4, Lower);

				SetIndexStyle(0, DRAW_LINE, STYLE_SOLID);       // Sets the shape, style, width and color for the indicator line.
				SetIndexLabel(0, "T_Channel(" + CutoffPeriod + ").Bull");

				SetIndexStyle(1, DRAW_LINE, STYLE_SOLID);       // repeat for all 3 buffers
				SetIndexLabel(1, "T_Channel(" + CutoffPeriod + ").Bear");

				SetIndexStyle(2, DRAW_LINE, STYLE_SOLID);
				SetIndexLabel(2, "T_Channel(" + CutoffPeriod + ").Mixed");

				SetIndexStyle(3, DRAW_LINE, STYLE_SOLID);       // Sets the shape, style, width and color for the indicator line.
				SetIndexLabel(3, "T_Channel(" + CutoffPeriod + ").Upper");

				SetIndexStyle(4, DRAW_LINE, STYLE_SOLID);       // Sets the shape, style, width and color for the indicator line.
				SetIndexLabel(4, "T_Channel(" + CutoffPeriod + ").Lower");

				// Sets the "short" name of a custom indicator to be shown in the DataWindow and in the chart subwindow.
				IndicatorShortName("T_Channel(" + CutoffPeriod + "," + SlopeThreshold + ")");

				priceDecycle = new Decycler_obj(IndPeriod, SlopeThreshold, CutoffPeriod);
				priceDecycleSmooth = new SUPERSMOOTHER_3Pole_obj(IndPeriod, SlopeThreshold);
				priceSmooth = new SUPERSMOOTHER_3Pole_obj(IndPeriod, SlopeThreshold);
				//deviationMean = new SMAobj(CutoffPeriod);
				deviationMean = new ALMAobj((CutoffPeriod*5), SlopeThreshold);
				

				Print("T_Channel: Started. [" + Chart.Symbol + "] tf=" + Period());      // Print this message to Alveo Log file on startup
			}
			catch (Exception ex)
			{
				Print("T_Channel: Init: Exception: " + ex.Message);
				Print("T_Channel: " + ex.StackTrace);
			}
			return 0;   // done
		}


		//+------------------------------------------------------------------+");
		//| Custom indicator deinitialization function                       |");
		//| Called by Alveo when the Indicator is closed                     |");
		//+------------------------------------------------------------------+");
		protected override int Deinit()
		{
			// ENTER YOUR CODE HERE
			return 0;
		}

		//+--------------------------------------------------------------------------+");
		//| Custom indicator iteration function                                      |");
		//| Called by Alveo everytime a new chart bar appears, and maybe more often  |");
		//+--------------------------------------------------------------------------+");
		protected override int Start()
		{
			try  // to catch and handle Exceptions that might occur in this code block
			{
				e = Bars - 1;   // e = largest index in ChartBars array
				if (e < 2)      // not enough data
					return -1;
				prevBar = b;

				if (firstrun)   // on first run, calculate DTEMA on all ChartBars data
				{
					b = ChartBars[e - 1];           // b = refernec to oldest ChartBars data
					prevBar = b;
					thePrice = (double)(b.Open + b.Close) / 2.0;     // initialize DTEMA to thePrice of the oldest bar

					priceDecycleSmooth.Init(thePrice);
					priceDecycle.Init(thePrice);
					deviationMean.Init(Deviation);

					firstrun = false;               // firstrun initialization completed 
				}
				else // not firstrun; only calculate DTEMA on ChartBars not already processed
				{
					counted_bars = IndicatorCounted();
					if (counted_bars < 0)
						throw new Exception("SSmooth: Start: invalid IndicatorCounted value.");            // invalid value
					e -= counted_bars;          // reduce e count by bars previously processed
				}

				if (e < 1)                      // no data to process
					return 0;                   // we're out of here, for now    

				for (int i = 0; i < e; i++)     // iterate each bar to process  
				{
					b = ChartBars[e - i - 1];                                   // get oldest chart bar in array
					if (prevBar == null)
						prevBar = b;
					thePrice = (double)(b.Open + b.Close) / 2.0;
					var gap = Math.Abs((double)(prevBar.Close - b.Open));
					prevBar = b;
					//if (gap > 50 * Point)
					//{
					//    Print("SSmooth: Gap detected. gap=" + gap / Point + " points. " + b.BarTime.ToString());
					//
					//    SSmoother_three.Init(thePrice);
					//}

					priceSmooth.Calc(((double)b.Open + (double)b.Low + (double)b.High + (double)b.Close) / 4);
					priceDecycle.Calc(b);
					priceDecycleSmooth.Calc(priceDecycle.PriceDecycle);  

					var AbsDifference = Math.Abs(priceSmooth.PriceSmooth - priceDecycleSmooth.PriceSmooth);
					max_AbsDifference = Math.Max(AbsDifference, prev_max_AbsDifference);
					prev_max_AbsDifference = max_AbsDifference;

					Deviation = priceSmooth.PriceSmooth - priceDecycleSmooth.PriceSmooth;
					DeviationAbs = Math.Abs(Deviation);
					MeanDeviation = deviationMean.Calc(DeviationAbs);
					bandOffset = Offset_Factor * MeanDeviation;

					UpdateBuffers(i);                                           // update Alvelo Indicator buffers
				}
			}
			catch (Exception ex)    // catch and Print any exceptions that may have happened
			{
				Print("SSmooth: Start: Exception: " + ex.Message);
				Print("SSmooth: " + ex.StackTrace);
			}
			return 0;
		}

		/// <summary>  
		///  UpdateBuffers - update Alveo Indicator buffers with new data
		/// <param name="entry">index value into buffer.</param>
		/// </summary>

		internal void UpdateBuffers(int entry)
		{
			var Decycle_Smooth = priceDecycleSmooth.PriceSmooth;
			var Price_Smooth = priceSmooth.PriceSmooth;

			double offset;
			string currentSymbol = Symbol();
			offset = currentSymbol.StartsWith("JPY") || currentSymbol.EndsWith("JPY") ? Min_Offset / 100 : Min_Offset / 10000;

			var indx = e - entry - 1;               // put data into buffers in the reverse order, for Alveo.
			UpTrend[indx] = EMPTY_VALUE;            // Initialize with EMPTY_VALUE
			DownTrend[indx] = EMPTY_VALUE;
			Consolidation[indx] = EMPTY_VALUE;

			Upper[indx] = EMPTY_VALUE;
			Lower[indx] = EMPTY_VALUE;

			Upper[indx] = Decycle_Smooth + Math.Max(bandOffset, offset);
			Lower[indx] = Decycle_Smooth - Math.Max(bandOffset, offset);


			if (priceDecycleSmooth.isRising)   // if UpTrend
			{
				UpTrend[indx] = Decycle_Smooth;             // UpTrend buffer gets ALMAvalue
			}
			else if (priceDecycleSmooth.isFalling) // Downtrend
			{
				DownTrend[indx] = Decycle_Smooth;           // DownTrend buffer gets ALMAvalue
			}
			else  // otherwise
			{
				Consolidation[indx] = Decycle_Smooth;       // Consolidation buffer gets ALMAvalue
			}

			if (priceDecycleSmooth.justChangedDir)                    // if trendDir changed from previous call
			{
				switch (priceDecycleSmooth.prevState)             // place connecting ALMA into proper buffer to connect the lines
				{
					case 1: // uptrend
						UpTrend[indx] = Decycle_Smooth;
						break;
					case -1: // downtrend
						DownTrend[indx] = Decycle_Smooth;
						break;
					case 0:  // consolidation
						Consolidation[indx] = Decycle_Smooth;
						break;
				}
			}

		}

		//+------------------------------------------------------------------+
		//| AUTO GENERATED CODE. THIS METHODS USED FOR INDICATOR CACHING     |
		//+------------------------------------------------------------------+
		#region Auto Generated Code

		[Description("Parameters order Symbol, TimeFrame")]
		public override bool IsSameParameters(params object[] values)  // determine if Indicator parameter values have not changed.
		{
			if (values.Length != 4)
				return false;

			if (!CompareString(Symbol, (string)values[0]))
				return false;

			if (TimeFrame != (int)values[1])
				return false;

			if (IndPeriod != (int)values[2])
				return false;

			if (SlopeThreshold != (double)values[3])
				return false;

			return true;
		}

		[Description("Parameters order Symbol, TimeFrame")]
		public override void SetIndicatorParameters(params object[] values)     // Set Indicator values from cache
		{
			if (values.Length != 4)
				throw new ArgumentException("Invalid parameters number");

			Symbol = (string)values[0];
			TimeFrame = (int)values[1];
			IndPeriod = (int)values[2];
			SlopeThreshold = (int)values[3];
		}

		#endregion  // Auto Generated Code

		//3-Pole Super Smoother 
		internal class SUPERSMOOTHER_3Pole_obj
		{
			//public double alma;
			//public double wSum;
			private bool firstRun;
			//Queue<double> Qprices;

			//public double Period;
			internal double velocity;
			//public double acceleration;
			//public double prev_acceleration;
			internal int prevState;

			internal bool isRising;
			internal bool isFalling;
			internal bool velocity_isRising;
			//internal bool velocity_isRising_Exit;
			internal bool velocity_isFalling;
			//internal bool velocity_isFalling_Exit;
			internal double Threshold;
			//internal double Threshold_Exit;
			internal bool justChangedDir;
			internal bool justChangedDir_Exit;
			//internal double velocity_Threshold;
			internal int dir;
			internal int vel_dir;

			internal double PriceSmooth;
			internal double PriceSmooth_Prev1;
			internal double PriceSmooth_Prev2;
			internal double PriceSmooth_Prev3;

			//Variables required for 3-Pole Super Smoother
			internal int SuperSmootherPeriod;
			internal double a1;
			internal double b1;
			internal double c1;
			internal double coef2;
			internal double coef3;
			internal double coef4;
			internal double coef1;

			double priceHigh;
			double priceLow;
			double priceClose;
			double priceOpen;
			double thePrice;

			const string dataFileDir = "C:\\temp\\";
			System.IO.StreamWriter dfile = null;

			// Alma constructor
			internal SUPERSMOOTHER_3Pole_obj()
			{
				//Period = input;
				//value = int.MinValue;
				PriceSmooth = double.MinValue;
				PriceSmooth_Prev1 = double.MinValue;
				PriceSmooth_Prev2 = double.MinValue;
				PriceSmooth_Prev3 = double.MinValue;
				//alma = double.MinValue;
				//wSum = double.MinValue;
				//Qprices = new Queue<double>();
				prevState = 0;
				firstRun = true;
				isRising = false;
				isFalling = false;
				velocity_isRising = false;
				velocity_isFalling = false;
				justChangedDir = false;
				justChangedDir_Exit = false;
				velocity = double.MinValue;
				//acceleration = double.MinValue;
				//prev_acceleration = double.MinValue;

			}

			// Setup of ALMA Object
			internal SUPERSMOOTHER_3Pole_obj(int period, int threshold) : this()
			{
				SuperSmootherPeriod = period;
				Threshold = (double)threshold * 1e-6;
				//K = 2.0 / (Period + 1.0);
			}

			// Initialize Alma
			internal void Init(double price)
			{
				//alma = double.MinValue;
				//wSum = double.MinValue;
				//Qprices.Clear();
				//Qprices.Enqueue(price);
				//prevValue = double.MinValue;
				//prevValue_two = double.MinValue;
				//prevValue_three = double.MinValue;

				//velocity = double.MinValue;
				//acceleration = double.MinValue;
				//prev_acceleration = double.MinValue;
				//int Period_int = (int)Period;
				//prevState = 0;
				//Threshold = 15 * 1e-6;
				//Threshold_Exit = 5 * 1e-6;
				//velocity_Threshold = 45 * 1e-6;
				//for (int i = 0; i <= Period_int; i++)
				//{
				//    Qprices.Enqueue(price);
				//}

				prevState = 0;

				// initialise 3-Pole Super Smoother Variables
				a1 = Math.Exp(-3.1459 / SuperSmootherPeriod);
				b1 = 2 * a1 * Math.Cos(Math.PI * (1.738 * 180 / SuperSmootherPeriod) / 180.0);

				c1 = a1 * a1;
				coef2 = b1 + c1;
				coef3 = -(c1 + b1 * c1);
				coef4 = c1 * c1;
				coef1 = 1 - coef2 - coef3 - coef4;

				PriceSmooth = price;
				PriceSmooth_Prev1 = price;
				PriceSmooth_Prev2 = price;
				PriceSmooth_Prev3 = price;

				return;
			}

			//Alma Calculation
			internal double Calc(double thePrice)
			{
				//priceLow = (double)theBar.Low;
				//priceHigh = (double)theBar.High;
				//priceClose = (double)theBar.Close;
				//priceOpen = (double)theBar.Open;
				//thePrice = (priceOpen + priceLow + priceHigh + priceClose) / 4;

				if (SuperSmootherPeriod < 1)
					throw new Exception("Almacalc: period < 1 invalid !!");

				if (firstRun)
				{
					Init(thePrice);
					firstRun = false;
				}

				else     // !firstrun
				{
					//if (pos == 0)
					//{
					//    thePrice = (priceOpen + priceLow + priceHigh) / 3;
					//    PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;
					//}

					//if (pos != 0)
					//{


					PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;

					velocity = PriceSmooth - PriceSmooth_Prev1;

					PriceSmooth_Prev3 = PriceSmooth_Prev2;
					PriceSmooth_Prev2 = PriceSmooth_Prev1;
					PriceSmooth_Prev1 = PriceSmooth;

					//price direction
					//velocity = value - prevValue;
					//acceleration = velocity - (prevValue_two - prevValue_three);

					//prev_acceleration = acceleration;

					justChangedDir = false;
					justChangedDir_Exit = false;
					var previous = isRising;
					var previous_exit = isRising;

					//Threshold_Exit = 0.8 * Threshold;
					isRising = (velocity > Threshold);
					//isRising = (value > prevValue);
					//isRising = (value > prevValue && prevValue > prevValue_two);
					//if(value > prevValue)
					//    { isRising = true; }

					//velocity_isRising_Exit = (velocity > Threshold_Exit);

					if (isRising && !previous)
						justChangedDir = true;
					if (isRising && !previous_exit)
						justChangedDir_Exit = true;

					previous = isFalling;
					//previous_exit = isFalling;

					isFalling = (velocity < -Threshold);
					//isFalling = (value < prevValue);
					//isFalling = (value < prevValue && prevValue < prevValue_two);

					//velocity_isFalling = (velocity < -Threshold);

					if (isFalling && !previous)
						justChangedDir = true;

					//isFalling = (velocity < -Threshold);
					//if (isFalling && !previous)
					//justChangedDir_Exit = true;

					prevState = dir;
					dir = isRising ? 1 : (isFalling ? -1 : 0);


					//Velocity direction
					//velocity_isFalling = (acceleration < -velocity_Threshold);
					//velocity_isRising = (acceleration > velocity_Threshold);

					vel_dir = velocity_isRising ? 1 : (velocity_isFalling ? -1 : 0);

					//value = Period;

					//prevValue_three = prevValue_two;
					//prevValue_two = prevValue;
					//prevValue = value;
					//}
					//}

				}
				return PriceSmooth;

			}

			internal void dumpData(string line, bool append = true)
			{
				if (!System.IO.Directory.Exists(dataFileDir))
					System.IO.Directory.CreateDirectory(dataFileDir);
				var filename = "ALMAdump.csv";
				dfile = new System.IO.StreamWriter(dataFileDir + filename, append);
				dfile.WriteLine(line);
				dfile.Close();
			}

		}

		//Decycle_obj 
		internal class Decycler_obj
		{
			//public double alma;
			//public double wSum;
			private bool firstRun;
			//Queue<double> Qprices;

			//public double Period;
			internal double velocity;
			//public double acceleration;
			//public double prev_acceleration;
			internal int prevState;

			internal bool isRising;
			internal bool isFalling;
			internal bool velocity_isRising;
			//internal bool velocity_isRising_Exit;
			internal bool velocity_isFalling;
			//internal bool velocity_isFalling_Exit;
			internal double Threshold;
			//internal double Threshold_Exit;
			internal bool justChangedDir;
			internal bool justChangedDir_Exit;
			//internal double velocity_Threshold;
			internal int dir;
			internal int vel_dir;

			internal double PriceSmooth;
			internal double PriceSmooth_Prev1;
			internal double PriceSmooth_Prev2;
			internal double PriceSmooth_Prev3;

			//Variables required for 3-Pole Super Smoother
			internal int SuperSmootherPeriod;
			internal double a1;
			internal double b1;
			internal double c1;
			internal double coef2;
			internal double coef3;
			internal double coef4;
			internal double coef1;

			double priceHigh;
			double priceLow;
			double priceClose;
			double priceOpen;
			double thePrice;
			double thePrice_Prev;

			//Variables Required for Decycler
			internal double alpha1;
			internal int Cutoff;
			internal double PriceDecycle;
			internal double PriceDecycle_Prev1;

			const string dataFileDir = "C:\\temp\\";
			System.IO.StreamWriter dfile = null;

			// Alma constructor
			internal Decycler_obj()
			{
				//Period = input;
				//value = int.MinValue;
				thePrice = double.MinValue;
				thePrice_Prev = double.MinValue;
				PriceSmooth = double.MinValue;
				PriceSmooth_Prev1 = double.MinValue;
				PriceSmooth_Prev2 = double.MinValue;
				PriceSmooth_Prev3 = double.MinValue;
				//alma = double.MinValue;
				//wSum = double.MinValue;
				//Qprices = new Queue<double>();
				prevState = 0;
				firstRun = true;
				isRising = false;
				isFalling = false;
				velocity_isRising = false;
				velocity_isFalling = false;
				justChangedDir = false;
				justChangedDir_Exit = false;
				velocity = double.MinValue;
				alpha1 = double.MinValue;
				PriceDecycle = double.MinValue;
				PriceDecycle_Prev1 = double.MinValue;
				//acceleration = double.MinValue;
				//prev_acceleration = double.MinValue;

			}

			// Setup of ALMA Object
			internal Decycler_obj(int period, int threshold, int cutoff) : this()
			{
				SuperSmootherPeriod = period;
				Cutoff = cutoff;
				Threshold = (double)threshold * 1e-6;
				//K = 2.0 / (Period + 1.0);
			}

			// Initialize Alma
			internal void Init(double price)
			{
				//alma = double.MinValue;
				//wSum = double.MinValue;
				//Qprices.Clear();
				//Qprices.Enqueue(price);
				//prevValue = double.MinValue;
				//prevValue_two = double.MinValue;
				//prevValue_three = double.MinValue;

				//velocity = double.MinValue;
				//acceleration = double.MinValue;
				//prev_acceleration = double.MinValue;
				//int Period_int = (int)Period;
				//prevState = 0;
				//Threshold = 15 * 1e-6;
				//Threshold_Exit = 5 * 1e-6;
				//velocity_Threshold = 45 * 1e-6;
				//for (int i = 0; i <= Period_int; i++)
				//{
				//    Qprices.Enqueue(price);
				//}

				prevState = 0;

				// initialise 3-Pole Super Smoother Variables
				a1 = Math.Exp(-3.1459 / SuperSmootherPeriod);
				b1 = 2 * a1 * Math.Cos(Math.PI * (1.738 * 180 / SuperSmootherPeriod) / 180.0);

				c1 = a1 * a1;
				coef2 = b1 + c1;
				coef3 = -(c1 + b1 * c1);
				coef4 = c1 * c1;
				coef1 = 1 - coef2 - coef3 - coef4;

				PriceSmooth = price;
				PriceSmooth_Prev1 = price;
				PriceSmooth_Prev2 = price;
				PriceSmooth_Prev3 = price;

				PriceDecycle = price;
				PriceDecycle_Prev1 = price;

				thePrice_Prev = price;

				return;
			}

			//Alma Calculation
			internal double Calc(Bar theBar)
			{
				priceLow = (double)theBar.Low;
				priceHigh = (double)theBar.High;
				priceClose = (double)theBar.Close;
				priceOpen = (double)theBar.Open;
				thePrice = (priceOpen + priceLow + priceHigh + priceClose) / 4;

				if (SuperSmootherPeriod < 1)
					throw new Exception("Almacalc: period < 1 invalid !!");

				if (firstRun)
				{
					Init(thePrice);
					firstRun = false;
				}

				else     // !firstrun
				{
					//if (pos == 0)
					//{
					//    thePrice = (priceOpen + priceLow + priceHigh) / 3;
					//    PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;
					//}

					//if (pos != 0)
					//{

					//alpha1 = (Cosine(360 / Cutoff) + Sine (360 / Cutoff) - 1) / Cosine(360 / Cutoff);
					//alpha1 = (Cosine(360 / Cutoff) + Sine(360 / Cutoff) - 1) / Cosine(360 / Cutoff);
					alpha1 = (Math.Cos(Math.PI * (360 / Cutoff) / 180.0) + Math.Sin(Math.PI * (360 / Cutoff) / 180) - 1) / (Math.Cos(Math.PI * (360 / Cutoff) / 180));
					//Decycle = (alpha1 / 2) * (Close + Close[1]) + (1 - alpha1) * De cycle[1];
					PriceDecycle = (alpha1 / 2) * (thePrice + thePrice_Prev) + (1 - alpha1) * PriceDecycle_Prev1;

					PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;

					velocity = PriceDecycle - PriceDecycle_Prev1;

					PriceSmooth_Prev3 = PriceSmooth_Prev2;
					PriceSmooth_Prev2 = PriceSmooth_Prev1;
					PriceSmooth_Prev1 = PriceSmooth;

					thePrice_Prev = thePrice;
					PriceDecycle_Prev1 = PriceDecycle;

					//price direction
					//velocity = value - prevValue;
					//acceleration = velocity - (prevValue_two - prevValue_three);

					//prev_acceleration = acceleration;

					justChangedDir = false;
					justChangedDir_Exit = false;
					var previous = isRising;
					var previous_exit = isRising;

					//Threshold_Exit = 0.8 * Threshold;
					isRising = (velocity > Threshold);
					//isRising = (value > prevValue);
					//isRising = (value > prevValue && prevValue > prevValue_two);
					//if(value > prevValue)
					//    { isRising = true; }

					//velocity_isRising_Exit = (velocity > Threshold_Exit);

					if (isRising && !previous)
						justChangedDir = true;
					if (isRising && !previous_exit)
						justChangedDir_Exit = true;

					previous = isFalling;
					//previous_exit = isFalling;

					isFalling = (velocity < -Threshold);
					//isFalling = (value < prevValue);
					//isFalling = (value < prevValue && prevValue < prevValue_two);

					//velocity_isFalling = (velocity < -Threshold);

					if (isFalling && !previous)
						justChangedDir = true;

					//isFalling = (velocity < -Threshold);
					//if (isFalling && !previous)
					//justChangedDir_Exit = true;

					prevState = dir;
					dir = isRising ? 1 : (isFalling ? -1 : 0);


					//Velocity direction
					//velocity_isFalling = (acceleration < -velocity_Threshold);
					//velocity_isRising = (acceleration > velocity_Threshold);

					vel_dir = velocity_isRising ? 1 : (velocity_isFalling ? -1 : 0);

					//value = Period;

					//prevValue_three = prevValue_two;
					//prevValue_two = prevValue;
					//prevValue = value;
					//}
					//}

				}
				return PriceDecycle;

			}

			internal void dumpData(string line, bool append = true)
			{
				if (!System.IO.Directory.Exists(dataFileDir))
					System.IO.Directory.CreateDirectory(dataFileDir);
				var filename = "ALMAdump.csv";
				dfile = new System.IO.StreamWriter(dataFileDir + filename, append);
				dfile.WriteLine(line);
				dfile.Close();
			}

		}
		
		//Alma - moving average
		internal class ALMAobj
		{
			public double alma;
			public double wSum;
			private bool firstRun;
			Queue<double> Qprices;
			internal double value;
			public double Period;
			internal double velocity;
			public double acceleration;
			public double prev_acceleration;
			internal int prevState;

			internal bool isRising;
			internal bool isFalling;
			internal bool velocity_isRising;
			internal bool velocity_isRising_Exit;
			internal bool velocity_isFalling;
			internal bool velocity_isFalling_Exit;
			internal double Threshold;
			internal double Threshold_Exit;
			internal bool justChangedDir;
			internal bool justChangedDir_Exit;
			internal double velocity_Threshold;
			internal int dir;
			internal int vel_dir;
			internal double prevValue;
			internal double prevValue_two;
			internal double prevValue_three;

			const string dataFileDir = "C:\\temp\\";
			System.IO.StreamWriter dfile = null;

			// Alma constructor
			internal ALMAobj()
			{
				//Period = input;
				value = int.MinValue;
				prevValue = double.MinValue;
				prevValue_two = double.MinValue;
				prevValue_three = double.MinValue;
				prevValue_three = double.MinValue;
				alma = double.MinValue;
				wSum = double.MinValue;
				Qprices = new Queue<double>();
				prevState = 0;
				firstRun = true;
				isRising = false;
				isFalling = false;
				velocity_isRising = false;
				velocity_isFalling = false;
				justChangedDir = false;
				justChangedDir_Exit = false;
				velocity = double.MinValue;
				acceleration = double.MinValue;
				prev_acceleration = double.MinValue;
			}

			// Setup of ALMA Object
			internal ALMAobj(double period, int threshold) : this()
			{
				Period = period;
				Threshold = (double)threshold * 1e-6;
				//K = 2.0 / (Period + 1.0);
			}

			// Initialize Alma
			internal void Init(double price)
			{
				alma = double.MinValue;
				wSum = double.MinValue;
				Qprices.Clear();
				Qprices.Enqueue(price);
				prevValue = double.MinValue;
				prevValue_two = double.MinValue;
				prevValue_three = double.MinValue;
				velocity = double.MinValue;
				acceleration = double.MinValue;
				prev_acceleration = double.MinValue;
				int Period_int = (int)Period;
				prevState = 0;
				//Threshold = 15 * 1e-6;
				//Threshold_Exit = 5 * 1e-6;
				velocity_Threshold = 45 * 1e-6;
				for (int i = 0; i <= Period_int; i++)
				{
					Qprices.Enqueue(price);
				}
				return;

			}

			//Alma Calculation
			internal double Calc(double Price)
			{
				double m = Math.Floor(1.15 * (Period));
				double s = Period / 6.0;
				alma = 0;
				wSum = 0;
				double w = 0;
				int Period_int = (int)Period;

				if (Period < 1)
					throw new Exception("Almacalc: period < 1 invalid !!");
				if (firstRun)
				{
					Init(Price);
					Qprices.Enqueue(Price);
					firstRun = false;

					for (int i = 0; i <= Period_int; i++)
					{
						Qprices.Enqueue(Price);
					}

					return Price;

				}
				else  // !firstrun
				{
					value = 0;
					/*
					if (pos == 0 || pos == 1)
					{
						var arr = Qprices.ToArray();
						var count = arr.Count();

						if (count > 1)

							w = Math.Exp(-(0 - m) * (0 - m) / (2 * s * s));
						alma += Price * w;
						wSum += w;

						for (int i = 0; i < (count - 2); i++)
						{
							w = Math.Exp(-((i + 1) - m) * ((i + 1) - m) / (2 * s * s));
							alma += arr[i] * w;
							wSum += w;
						}
						value = alma / wSum;

						return value;
					}
					*/
					//if (pos != 0 || pos != 1)
					{
						Qprices.Enqueue(Price);
						while (Qprices.Count > (Period + 1))
						{
							Qprices.Dequeue();
						}

						var arr = Qprices.ToArray();
						var count = arr.Count();

						if (count > 1)
						{
							//double sum = arr.Sum();
							//value = sum / count;
							for (int i = 0; i < (count - 1); i++)
							//for (int i = (count-1); i >=0; i--) 
							{
								w = Math.Exp(-(i - m) * (i - m) / (2 * s * s));
								alma += arr[i] * w;
								wSum += w;
							}
						}

						value = alma / wSum;



						//price direction
						velocity = value - prevValue;
						acceleration = velocity - (prevValue_two - prevValue_three);

						//prev_acceleration = acceleration;

						justChangedDir = false;
						justChangedDir_Exit = false;
						var previous = isRising;
						var previous_exit = isRising;

						Threshold_Exit = 0.8 * Threshold;


						isRising = (velocity > Threshold);
						//isRising = (value > prevValue);
						//isRising = (value > prevValue && prevValue > prevValue_two);
						//if(value > prevValue)
						//    { isRising = true; }

						//velocity_isRising_Exit = (velocity > Threshold_Exit);

						if (isRising && !previous)
							justChangedDir = true;
						if (isRising && !previous_exit)
							justChangedDir_Exit = true;

						previous = isFalling;
						//previous_exit = isFalling;

						isFalling = (velocity < -Threshold);
						//isFalling = (value < prevValue);
						//isFalling = (value < prevValue && prevValue < prevValue_two);

						//velocity_isFalling = (velocity < -Threshold);

						if (isFalling && !previous)
							justChangedDir = true;

						//isFalling = (velocity < -Threshold);
						//if (isFalling && !previous)
						//    justChangedDir_Exit = true;

						prevState = dir;
						dir = isRising ? 1 : (isFalling ? -1 : 0);


						//Velocity direction
						//velocity_isFalling = (acceleration < -velocity_Threshold);
						//velocity_isRising = (acceleration > velocity_Threshold);

						vel_dir = velocity_isRising ? 1 : (velocity_isFalling ? -1 : 0);

						//value = Period;

						prevValue_three = prevValue_two;
						prevValue_two = prevValue;
						prevValue = value;

					}
					return value;
				}
			}

			internal void dumpData(string line, bool append = true)
			{
				if (!System.IO.Directory.Exists(dataFileDir))
					System.IO.Directory.CreateDirectory(dataFileDir);
				var filename = "ALMAdump.csv";
				dfile = new System.IO.StreamWriter(dataFileDir + filename, append);
				dfile.WriteLine(line);
				dfile.Close();
			}

		}
	}
}