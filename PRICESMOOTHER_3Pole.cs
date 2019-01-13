
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
    public class PRICESMOOTHER_3Pole : IndicatorBase
    {
		public enum PriceTypes
		{
			PRICE_CLOSE = 0,
			PRICE_OPEN = 1,
			PRICE_HIGH = 2,
			PRICE_LOW = 3,
			PRICE_MEDIAN = 4,
			PRICE_TYPICAL = 5,
			PRICE_WEIGHTED = 6,
			PRICE_OHLC = 7,
			PRICE_P7 = 8
		}

		#region Properties
		// User settable Properties for this Alveo Indicator
		/// <param name="period">Sets the strength of the filtering.</param>
		[Category("Settings")]
        [DisplayName("MA Period. ")]
        [Description("Sets the strength of the filtering. [ex: 20]")]
        public int IndPeriod { get; set; }

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

        Bar b;              // holds latest chart Bar data from Alveo
        Bar prevBar;        // holds latest chart Bar data from Alveo
        int counted_bars;   // amount of bars of bars on chart already processed by the indicator.
        int e;              // number of bars for the indicator to calculate
        double thePrice;    // holds the currency pair price for the NDI calculation. In units of the bas currency.
        bool firstrun;      // firstrun = true on first execution of the Start function. False otherwise.
		public PriceTypes PriceType = PriceTypes.PRICE_P7;
		//public PriceTypes PriceType = PriceTypes.PRICE_OHLC;

		SUPERSMOOTHER_3Pole_obj SSmoother_three;

		const string dataFileDir = "C:\\temp\\ALMA\\";

        /// <summary>  
        ///  C# constructor for ALMA Class
        ///  called to initialize the class when class is created by Alveo
        /// </summary>  
        public PRICESMOOTHER_3Pole()
        {
            try
            {
                // Basic indicator initialization. Don't use this constructor to calculate values

                indicator_buffers = 3;              // 3 lines on Alveo chart require 3 buffers
                indicator_chart_window = true;
                firstrun = true;                    // initially set true for first run
                prevBar = null;

                IndPeriod = 7;                     // Initial value for DTEMAv2 period
                SlopeThreshold = 10;                // Initial value for slopeThreshold

                indicator_width1 = 1;               // width of line 1 on the chart
                indicator_width2 = 1;
                indicator_width3 = 1;
                indicator_color1 = Colors.White;     // line colors
                indicator_color2 = Colors.White;
                indicator_color3 = Colors.White;

                UpTrend = new Array<double>();      // 3 data buffers for Alveo Indicator
                DownTrend = new Array<double>();
                Consolidation = new Array<double>();

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

                SetIndexStyle(0, DRAW_LINE, STYLE_SOLID);       // Sets the shape, style, width and color for the indicator line.
                SetIndexLabel(0, "SSmooth(" + IndPeriod + ").Bull");

                SetIndexStyle(1, DRAW_LINE, STYLE_SOLID);       // repeat for all 3 buffers
                SetIndexLabel(1, "SSmooth(" + IndPeriod + ").Bear");

                SetIndexStyle(2, DRAW_LINE, STYLE_SOLID);
                SetIndexLabel(2, "SSmooth(" + IndPeriod + ").Mixed");

                // Sets the "short" name of a custom indicator to be shown in the DataWindow and in the chart subwindow.
                IndicatorShortName("SSmooth(" + IndPeriod + "," + SlopeThreshold + ")");

                SSmoother_three = new SUPERSMOOTHER_3Pole_obj(IndPeriod, SlopeThreshold, (int)PriceType);

                Print("SSmooth: Started. [" + Chart.Symbol + "] tf=" + Period());      // Print this message to Alveo Log file on startup
            }
            catch (Exception ex)
            {
                Print("SSmooth: Init: Exception: " + ex.Message);
                Print("SSmooth: " + ex.StackTrace);
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
				var pos = Bars - IndicatorCounted();                // how many bars to be processed
				e = Bars - 1;   // e = largest index in ChartBars array
                if (e < 2)      // not enough data
                    return -1;
                prevBar = b;

                if (firstrun)   // on first run, calculate DTEMA on all ChartBars data
                {
                    b = ChartBars[e - 1];           // b = refernec to oldest ChartBars data
                    prevBar = b;
                    thePrice = (double)(b.Open + b.Close) / 2.0;     // initialize DTEMA to thePrice of the oldest bar

                    SSmoother_three.Init(thePrice);

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
                    if (gap > 50 * Point)
                    {
                        Print("SSmooth: Gap detected. gap=" + gap / Point + " points. " + b.BarTime.ToString());

                        SSmoother_three.Init(thePrice);
                    }

                    SSmoother_three.Calc(b);                                        // calculate new DTEMA value

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
            var smoother_value = SSmoother_three.PriceSmooth;

            var indx = e - entry - 1;               // put data into buffers in the reverse order, for Alveo.
            UpTrend[indx] = EMPTY_VALUE;            // Initialize with EMPTY_VALUE
            DownTrend[indx] = EMPTY_VALUE;
            Consolidation[indx] = EMPTY_VALUE;
            
            if (SSmoother_three.isRising)   // if UpTrend
            {
                UpTrend[indx] = smoother_value;             // UpTrend buffer gets ALMAvalue
            }
            else if (SSmoother_three.isFalling) // Downtrend
            {
                DownTrend[indx] = smoother_value;           // DownTrend buffer gets ALMAvalue
            }
            else  // otherwise
            {
                Consolidation[indx] = smoother_value;       // Consolidation buffer gets ALMAvalue
            }

            if (SSmoother_three.justChangedDir)                    // if trendDir changed from previous call
            {
                switch (SSmoother_three.prevState)             // place connecting ALMA into proper buffer to connect the lines
                {
                    case 1: // uptrend
                        UpTrend[indx] = smoother_value;
                        break;
                    case -1: // downtrend
                        DownTrend[indx] = smoother_value;
                        break;
                    case 0:  // consolidation
                        Consolidation[indx] = smoother_value;
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
            private bool firstRun;
            internal double velocity;
            internal int prevState;
            internal bool isRising;
            internal bool isFalling;
            internal bool velocity_isRising;
            internal bool velocity_isFalling;
            internal double Threshold;
            internal bool justChangedDir;
            internal int dir;
            internal int vel_dir;
			internal int priceType;

			double priceHigh;
			double priceLow;
			double priceClose;
			double priceOpen;
			double thePrice;
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

            const string dataFileDir = "C:\\temp\\";
            System.IO.StreamWriter dfile = null;

            // Smoother constructor
            internal SUPERSMOOTHER_3Pole_obj()
            {
                PriceSmooth = double.MinValue;
                PriceSmooth_Prev1 = double.MinValue;
                PriceSmooth_Prev2 = double.MinValue;
                PriceSmooth_Prev3 = double.MinValue;
                prevState = 0;
                firstRun = true;
                isRising = false;
                isFalling = false;
                velocity_isRising = false;
                velocity_isFalling = false;
                justChangedDir = false;
                velocity = double.MinValue;
            }

            // Setup of Smoother Object
            internal SUPERSMOOTHER_3Pole_obj(int period, int threshold, int type) : this()
            {
                SuperSmootherPeriod = period;
                Threshold = (double)threshold * 1e-6;
				priceType = type;
			}

            // Initialize Smoother
            internal void Init(double price)
            {
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

				thePrice = -1;

                return;
            }

            //Smoother Calculation
            internal double Calc(Bar theBar)
            {
                priceLow = (double)theBar.Low;
                priceHigh = (double)theBar.High;
                priceClose = (double)theBar.Close;
                priceOpen = (double)theBar.Open;

				switch (priceType)
				{

					case (int)PriceTypes.PRICE_CLOSE:
						thePrice = (double)theBar.Close;
						break;
					case (int)PriceTypes.PRICE_OPEN:
						thePrice = (double)theBar.Open;
						break;
					case (int)PriceTypes.PRICE_HIGH:
						thePrice = (double)theBar.High;
						break;
					case (int)PriceTypes.PRICE_LOW:
						thePrice = (double)theBar.Low;
						break;
					case (int)PriceTypes.PRICE_MEDIAN:
						thePrice = ((double)theBar.High + (double)theBar.Low) / 2;
						break;
					case (int)PriceTypes.PRICE_TYPICAL:
						thePrice = ((double)theBar.High + (double)theBar.Low + (double)theBar.Close) / 3;
						break;
					case (int)PriceTypes.PRICE_WEIGHTED:
						thePrice = ((double)theBar.High + (double)theBar.Low + 2 * (double)theBar.Close) / 4;
						break;
					case (int)PriceTypes.PRICE_OHLC:
						thePrice = Math.Round(((double)theBar.Open + (double)theBar.High + (double)theBar.Low + (double)theBar.Close) / 4, 5);
						break;
					case (int)PriceTypes.PRICE_P7:
						thePrice = Math.Round(((double)theBar.Open + (double)theBar.High + 2 * (double)theBar.Low + 3 * (double)theBar.Close) / 7, 5);
						break;
				}

				if (SuperSmootherPeriod < 1)
                    throw new Exception("Smoother: period < 1 invalid !!");

                if (firstRun)
                {
                    Init(thePrice);
                    firstRun = false;
                }

                else     // !firstrun
                {

                        PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;

                        velocity = PriceSmooth - PriceSmooth_Prev1;

                        PriceSmooth_Prev3 = PriceSmooth_Prev2;
                        PriceSmooth_Prev2 = PriceSmooth_Prev1;
                        PriceSmooth_Prev1 = PriceSmooth;

                        justChangedDir = false;
                        var previous = isRising;
                        var previous_exit = isRising;

                        isRising = (velocity > Threshold);
                        //isRising = (value > prevValue);
                        //isRising = (value > prevValue && prevValue > prevValue_two);

                        if (isRising && !previous)
                            justChangedDir = true;

                        previous = isFalling;

                        isFalling = (velocity < -Threshold);
                        //isFalling = (value < prevValue);
                        //isFalling = (value < prevValue && prevValue < prevValue_two);

                        if (isFalling && !previous)
                            justChangedDir = true;

                        prevState = dir;
                        dir = isRising ? 1 : (isFalling ? -1 : 0);

                        vel_dir = velocity_isRising ? 1 : (velocity_isFalling ? -1 : 0);
                    
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
    }
}