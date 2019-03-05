using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Alveo.Interfaces.UserCode;
using Alveo.Common.Classes;
using System.Windows.Media;



namespace Alveo.UserCode

{
	[Serializable]
	[Description("Frequency True Range Indicator")]

	public class FTR : IndicatorBase
	{

		internal class F_TR
		{
			//Alma - moving average
			internal class ALMAobj
			{
				public double alma;
				public double wSum;
				private bool firstRun;
				Queue<double> Qprices;
				internal double value;
				public int Period;
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
				internal ALMAobj(int period, double threshold) : this()
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
					double m = Math.Floor(1.2 * (Period));
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

						return value;

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
						//{
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

						//}
						return value;
					}
				}
			}

			//atr indicator
			public class FTRobj
			{
				//internal SMAobj sma;
				int IndPeriod;
				bool firstRun;
				internal double TR;
				internal double value;
				internal double prev_value;
				internal double prev_value2;
				Bar prevBar;

				//Variables required for 3-Pole Super Smoother
				internal int SuperSmootherPeriod;
				internal double a1;
				internal double b1;
				internal double c1;
				internal double coef2;
				internal double coef3;
				internal double coef4;
				internal double coef1;
				internal int SlopeThreshold;
				internal double trSmooth;
				internal double trSmooth_Prev1;
				internal double	trSmooth_Prev2;
				internal double trSmooth_Prev3;
				internal double tr_Change;
				internal double tr_Prev;

				//variables required for AGC
				internal double Peak;
				internal double Peak_Prev1;
				internal double Signal;

				internal FTRobj()
				{
					//sma = null;
					firstRun = true;
					TR = 0;
					value = 0;
					prev_value = value;
					prev_value2 = prev_value;
					prevBar = null;
				}
				internal FTRobj(int period) : this()
				{
					SuperSmootherPeriod = period;
					//sma = new SMAobj(period);
					// initialise 3-Pole Super Smoother Variables
					a1 = Math.Exp(-3.1459 / SuperSmootherPeriod);
					b1 = 2 * a1 * Math.Cos(Math.PI * (1.738 * 180 / SuperSmootherPeriod) / 180.0);
					c1 = a1 * a1;
					coef2 = b1 + c1;
					coef3 = -(c1 + b1 * c1);
					coef4 = c1 * c1;
					coef1 = 1 - coef2 - coef3 - coef4;
					trSmooth = 0;
					trSmooth_Prev1 = 0;
					trSmooth_Prev2 = 0;
					trSmooth_Prev3 = 0;
					tr_Change = 0;
					tr_Prev = 0;
				}
				internal void Init(Bar b = null)
				{
					//sma.Init(0);
					TR = 0;
					value = 0;
					prev_value = value;
					prev_value2 = prev_value;
					prevBar = b;

					// initialise AGC
					Peak = 0;
					Peak_Prev1 = 0;
					Signal = 0;
				}
				internal double Calc(Bar b)
				{
					if (firstRun)
					{
						Init(b);
						firstRun = false;
					}
					else
					{
						prev_value2 = prev_value;
						prev_value = value;
						value = 0;
						//TR = (double)Math.Max(b.bar.High - b.bar.Low), 1);
						TR = (double)Math.Max((b.High - b.Low),
								//TR = (double)Math.Max(Math.Abs(b.bar.High - b.bar.Low),
								//    Math.Max(Math.Abs(b.bar.High - prevBar.bar.Close), Math.Abs(prevBar.bar.Close - b.bar.Low)));
								Math.Max((Math.Abs(b.High - prevBar.Close)), Math.Abs(prevBar.Close - b.Low))); //
																												//value = sma.Calc(TR);

						//tr_Change = (TR - tr_Prev) / tr_Prev;
						//if (tr_Change > 2) tr_change = 2;
						//if (tr_Change < -2) tr_change = -2;

						trSmooth = coef1 * TR + coef2 * trSmooth_Prev1 + coef3 * trSmooth_Prev2 + coef4 * trSmooth_Prev3;

						//Peak = .991 * Peak[1]; If AbsValue(BP) > Peak Then Peak = AbsValue(BP); If Peak<> 0 Then Signal = BP / Peak;
						//Peak = 0.991 * Peak_Prev1;
						//trSmooth_change = (trSmooth - trSmooth_Prev1) / trSmooth_Prev1;
						//if (trSmooth_change > 2) trSmooth_change = 2;
						//if (trSmooth_change < -2) trSmooth_change = -2;
						//trSmooth_change = trSmooth_change / 2;
						//if ( Math.Abs(trSmooth_change) > Peak )
						//	Peak = Math.Abs(trSmooth_change);
						//if (Peak != 0)
						//	Signal = 2*trSmooth_change / Peak ;
						//trSmooth = trSmooth_Prev1 + Signal * trSmooth_Prev1;

						value = trSmooth;
						//value = Signal;
						prevBar = b;

						trSmooth_Prev3 = trSmooth_Prev2;
						trSmooth_Prev2 = trSmooth_Prev1;
						trSmooth_Prev1 = trSmooth;


					}
					return value;
				}
			}

			//Decycle indicator
			internal class Decycler_obj
			{
				private bool firstRun;

				//public double Period;
				internal double velocity;
				internal int prevState;

				internal bool isRising;
				internal bool isFalling;
				internal bool velocity_isRising;
				internal bool velocity_isFalling;
				internal double Threshold;
				internal bool justChangedDir;
				internal bool justChangedDir_Exit;
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
					thePrice = double.MinValue;
					thePrice_Prev = double.MinValue;
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
					justChangedDir_Exit = false;
					velocity = double.MinValue;
					alpha1 = double.MinValue;
					PriceDecycle = double.MinValue;
					PriceDecycle_Prev1 = double.MinValue;

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
						alpha1 = (Math.Cos(Math.PI * (360 / Cutoff) / 180.0) + Math.Sin(Math.PI * (360 / Cutoff) / 180) - 1) / (Math.Cos(Math.PI * (360 / Cutoff) / 180));
						PriceDecycle = (alpha1 / 2) * (thePrice + thePrice_Prev) + (1 - alpha1) * PriceDecycle_Prev1;

						PriceSmooth = coef1 * thePrice + coef2 * PriceSmooth_Prev1 + coef3 * PriceSmooth_Prev2 + coef4 * PriceSmooth_Prev3;

						velocity = PriceDecycle - PriceDecycle_Prev1;

						PriceSmooth_Prev3 = PriceSmooth_Prev2;
						PriceSmooth_Prev2 = PriceSmooth_Prev1;
						PriceSmooth_Prev1 = PriceSmooth;

						thePrice_Prev = thePrice;
						PriceDecycle_Prev1 = PriceDecycle;

						justChangedDir = false;
						justChangedDir_Exit = false;
						var previous = isRising;
						var previous_exit = isRising;

						isRising = (velocity > Threshold);

						if (isRising && !previous)
							justChangedDir = true;
						if (isRising && !previous_exit)
							justChangedDir_Exit = true;

						previous = isFalling;

						isFalling = (velocity < -Threshold);

						if (isFalling && !previous)
							justChangedDir = true;

						prevState = dir;
						dir = isRising ? 1 : (isFalling ? -1 : 0);

						vel_dir = velocity_isRising ? 1 : (velocity_isFalling ? -1 : 0);

					}
					return PriceDecycle;
				}
			}

			FTR ea;

			ALMAobj FTR_ALMA;
			FTRobj FTR;
			Decycler_obj FTRDecycle;

			internal double Threshold;
			int counter;

			internal double SlopeThreshold;
			internal double priceClose;
			internal double priceOpen;
			internal double priceAvg;
			internal double ftr;
			internal double ftr_alma;

			internal bool ftrisRising;
			internal bool ftrisFalling;
			internal int trendDir;
			internal int prevTrendDir;
			internal int prevState;
			internal bool trendChanged;
			internal bool stateChanged;

			internal bool ftr_low;

			internal F_TR()

			{
				Threshold = 0;
				priceClose = 0;
				priceOpen = 0;
				priceAvg = 0;
				ftr = 0;
				ftr_alma = 0;
				ftrisRising = false;
				ftrisFalling = false;
				trendDir = 0;
				prevTrendDir = 0;
				prevState = 0;
				trendChanged = false;
				stateChanged = false;
				ftr_low = false;
			}

			internal F_TR(FTR eaIn, int FTR_Period, int ALMA_Period) : this()
			{
				ea = eaIn;
				Threshold = (double)10 * 1e-7;
				FTR_ALMA = new ALMAobj(ALMA_Period, Threshold);
				FTR = new FTRobj(FTR_Period);
				//FTRDecycle = new Decycler_obj(FTR_Period, 10, ALMA_Period);
				FTRDecycle = new Decycler_obj(2, 10, ALMA_Period);
			}

			internal void Init(Bar b)

			{
				ea.Print("MACD2 Init.");
				counter = 0;

				priceClose = (double)b.Close;
				priceOpen = (double)b.Open;
				priceAvg = (priceClose + priceOpen) / 2;
				FTR_ALMA.Init(priceAvg);
				FTR.Init(b);

				ftr = 0;
				ftr_alma = 0;
				ftrisRising = false;
				ftrisFalling = false;
				trendDir = 0;
				prevTrendDir = 0;
				prevState = 0;
				trendChanged = false;
				stateChanged = false;
				ftr_low = false;
			}

			internal void Calc(Bar theBar)
			{

				//ea.Print(theBar.Close);
				if (theBar == null)
					throw new Exception("Stochastic.calc: theBar==null.");
				priceClose = (double)theBar.Close;
				priceOpen = (double)theBar.Open;
				priceAvg = (priceClose + priceOpen) / 2;

				ftr = FTR.Calc(theBar);
				ftr_alma = FTRDecycle.Calc(ftr);

				//indicator state
				ftrisRising = (ftr >= ftr_alma);
				ftrisFalling = (ftr < ftr_alma);
				if (trendDir != 0)
					prevTrendDir = trendDir;
				prevState = trendDir;
				trendDir = ftrisRising ? 1 : (ftrisFalling ? -1 : 0);
				trendChanged = (trendDir * prevTrendDir < 0);
				stateChanged = (trendDir != prevState);

				counter++;

			}

		}
	
		private readonly Array<double> _FTR_Up = new Array<double>();
		private readonly Array<double> _FTR_Dn = new Array<double>();
		private readonly Array<double> _FTR = new Array<double>();
		private readonly Array<double> _FTRALMA = new Array<double>();
		private readonly Array<double> _zero = new Array<double>();
		private readonly Array<double> _FTR_Low = new Array<double>();

		private int draw_begin1;
		private int draw_begin2;
		int nBuffered;
		F_TR filtertruerange;
		private double ftr;
		private double ftr_adjusted;

		public FTR()

		{
			nBuffered = 0;
			indicator_buffers = 6;
			indicator_chart_window = false;
			PriceType = PriceConstants.PRICE_CLOSE;

			DECYCLE_Period = 20;
			FTR_Period = 10;
			FTR_THRESHOLD = 0.00035;
			Levels.Values.Add(new Alveo.Interfaces.UserCode.Double(0.05));

			Color transparentRed = Color.FromArgb(50, 255, 0, 0); //127 / 255 = 50 % of transparency
			Color transparentGreen = Color.FromArgb(50, 0, 128, 0); //55 / 255 = 50 % of transparency
			Color transparentGolden = Color.FromArgb(50, 218, 165, 32); //55 / 255 = 50 % of transparency
			Color transparentWhite = Color.FromArgb(80, 255, 255, 255); //55 / 255 = 50 % of transparency

			indicator_color1 = Colors.Silver;
			indicator_color2 = transparentGreen;
			indicator_color3 = transparentRed;
			indicator_color4 = Colors.DeepSkyBlue;
			indicator_color5 = Colors.Silver;
			indicator_color6 = transparentGolden;
			indicator_color7 = transparentWhite;
			indicator_color8 = transparentWhite;

			var short_name = "FTR(" + FTR_Period + "," + DECYCLE_Period + ")";
			IndicatorShortName(short_name);
			SetIndexLabel(0, "FTR_DECYCLE");
			SetIndexLabel(1, "FTR_UP");
			SetIndexLabel(2, "FTR_DOWN");
			SetIndexLabel(3, "FTR");
			SetIndexLabel(4, "Zero");
			SetIndexLabel(5, "FTR_LOW");

		}

		#region UserSettings

		//FTR Variables
		[Description("Period of the smoothing filter applied to true range in Bars. [ex: 10]")]
		[Category("FTR Settings")]
		[DisplayName("FTR Smooth Period")]
		public int FTR_Period { get; set; }

		[Description("Period of the Decycler that averages FTR in Bars. [ex: 100]")]
		[Category("FTR Settings")]
		[DisplayName("DECYCLE Period")]
		public int DECYCLE_Period { get; set; }

		[Description("Threshold that defines low movement. [ex: 0.00035]")]
		[Category("FTR Settings")]
		[DisplayName("Low Movement Threshold")]
		public double FTR_THRESHOLD { get; set; }

		#endregion

		public PriceConstants PriceType { get; set; }

		protected override int Init()

		{
			nBuffered = 0;
			filtertruerange = new F_TR(this, FTR_Period, DECYCLE_Period);

			string short_name;

			SetIndexBuffer(0, _FTRALMA);
			SetIndexBuffer(1, _FTR_Up);
			SetIndexBuffer(2, _FTR_Dn);
			SetIndexBuffer(3, _FTR);
			SetIndexBuffer(4, _zero);
			SetIndexBuffer(5, _FTR_Low);

			SetIndexStyle(0, DRAW_LINE, 0, 1, indicator_color1);
			SetIndexStyle(1, DRAW_HISTOGRAM, 0, 2, indicator_color2);
			SetIndexStyle(2, DRAW_HISTOGRAM, 0, 2, indicator_color3);
			SetIndexStyle(3, DRAW_LINE, 0, 1, indicator_color4);
			SetIndexStyle(4, DRAW_LINE, 0, 3, indicator_color5);
			SetIndexStyle(5, DRAW_HISTOGRAM, 0, 2, indicator_color6);

			ArrayInitialize(_FTRALMA, 0.0);
			ArrayInitialize(_FTR_Up, EMPTY_VALUE);
			ArrayInitialize(_FTR_Dn, EMPTY_VALUE);
			ArrayInitialize(_FTR, 0.0);
			ArrayInitialize(_zero, 0.0);
			ArrayInitialize(_FTR_Low, EMPTY_VALUE);

			SetIndexDrawBegin(0, DECYCLE_Period);
			SetIndexDrawBegin(1, DECYCLE_Period);
			SetIndexDrawBegin(2, DECYCLE_Period);
			SetIndexDrawBegin(3, DECYCLE_Period);
			SetIndexDrawBegin(4, DECYCLE_Period);
			SetIndexDrawBegin(5, DECYCLE_Period);

			SetIndexLabel(0, "FTR_DECYCLE");
			SetIndexLabel(1, "FTR_UP");
			SetIndexLabel(2, "FTR_DOWN");
			SetIndexLabel(3, "FTR");
			SetIndexLabel(4, "Zero");
			SetIndexLabel(5, "FTR_LOW");

			return (0);

		}

		protected override int Start()
		{
			try
			{
				int i;
				var counted_bars = IndicatorCounted();
				Bar theBar;
				var e = Bars - counted_bars - 1;

				if (e < 1)                      // no data to process
					return 0;                   // we're out of here, for now    

				var data = GetHistory(Symbol, TimeFrame);
				if (data.Count == 0)
					return 0;

				Array<double> uptrend = new Array<double>();
				ArrayResize(uptrend, data.Count);
				ArraySetAsSeries(uptrend, true);


				if (nBuffered < 1)
				{
					var cntB = Bars;
					theBar = data[cntB - 1];
					filtertruerange.Init(theBar);
					for (i = 0; i < cntB; i++)
					{
						int pos = cntB - i - 1;
						theBar = data[cntB - i - 1];

						filtertruerange.Calc(theBar);

						_FTR[cntB - i - 1] = filtertruerange.ftr;
						_FTRALMA[cntB - i - 1] = filtertruerange.ftr_alma;

						uptrend[pos] = uptrend[pos + 1];

						if (_FTR[pos] > _FTRALMA[pos]) uptrend[pos] = 1;

						if (_FTR[pos] < _FTRALMA[pos]) uptrend[pos] = -1;

						if ( (_FTR[pos] < FTR_THRESHOLD) && (_FTRALMA[pos] < FTR_THRESHOLD)) uptrend[pos] = 0;
							

						if ((uptrend[pos] == 1))
						{
							_FTR_Up[pos] = _FTR[pos];
							_FTR_Dn[pos] = EMPTY_VALUE;
							_FTR_Low[pos] = EMPTY_VALUE;
						}

						if ((uptrend[pos] == -1))
						{
							_FTR_Dn[pos] = _FTR[pos];
							_FTR_Up[pos] = EMPTY_VALUE;
							_FTR_Low[pos] = EMPTY_VALUE;
						}

						if ((uptrend[pos] == 0))
						{
							_FTR_Dn[pos] = EMPTY_VALUE;
							_FTR_Up[pos] = EMPTY_VALUE;
							_FTR_Low[pos] = _FTR[pos];
						}

						_zero[pos] = 0;

						nBuffered++;
					}
				}

				else
				{
					
					var cnt = Bars - counted_bars;
					for (i = 0; i < cnt; i++)
					{
						int pos2 = cnt - i - 1;
						theBar = data[cnt - i - 1];

						filtertruerange.Calc(theBar);

						_FTR[cnt - i - 1] = filtertruerange.ftr;
						_FTRALMA[cnt - i - 1] = filtertruerange.ftr_alma;

						uptrend[pos2] = uptrend[pos2 + 1];

						if (_FTR[pos2] > _FTR[pos2 + 1]) uptrend[pos2] = 1;
						if (_FTR[pos2] < _FTR[pos2 + 1]) uptrend[pos2] = -1;

						if ((uptrend[pos2] == 1))
						{
							_FTR_Up[pos2] = _FTR[pos2];
							_FTR_Dn[pos2] = EMPTY_VALUE;
						}

						if ((uptrend[pos2] == -1))
						{
							_FTR_Dn[pos2] = _FTR[pos2];
							_FTR_Up[pos2] = EMPTY_VALUE;
						}

						_zero[pos2] = 0;

						nBuffered++;
					}

				}

			}

			catch (Exception e)
			{
				Print("Exception: " + e.Message);
				Print("Exception: " + e.StackTrace);
			}

			return (0);
		}

		public override bool IsSameParameters(params object[] values)

		{

			if (values.Length != 7)
				return false;
			if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
				return false;
			if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
				return false;
			if (!(values[1] is int) || (int)values[1] != TimeFrame)
				return false;
			//if (!(values[4] is int) || (int)values[4] != Threshold)
			//	return false;
			//if (!(values[5] is MovingAverageType) || (MovingAverageType)values[5] != MAType)
			//	return false;
			if (!(values[6] is PriceConstants) || (PriceConstants)values[6] != PriceType)
				return false;

			return true;

		}

	}

}