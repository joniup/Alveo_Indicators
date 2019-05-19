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
	[Description("Dual Stochastic indicator.")]

	public class FISHER_X : IndicatorBase
	{

		internal class FISHER_X_Obj
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
			//Rms              
			public class RMSobj
			{
				internal double mean;
				internal double sum;
				internal double rms;
				internal double delta;
				internal int IndPeriod;
				internal Queue<double> Qdelta;
				internal bool firstRun;
				internal double outsidebands_factor;
				internal double insidebands_factor;
				//internal int bands_factor;
				internal double prevValue;
				internal double prevValue_two;
				internal bool insidebands;
				internal bool prev_insidebands;
				internal bool prev_insidebands_two;
				internal bool justChanged_insidebands;
				internal bool justChanged_outsidebands;
				internal bool bullish;
				internal bool bearish;
				internal bool prevBullish;
				internal bool prevBearish;

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
				double rmsSmooth;
				double rmsSmooth_Prev1;
				double rmsSmooth_Prev2;
				double rmsSmooth_Prev3;

				// Bandsobj constructor
				internal RMSobj()
				{
					mean = 0;
					sum = 0;
					rms = 0;
					//period = alpha;
					firstRun = true;
					insidebands_factor = 0.8;
					outsidebands_factor = 0.9;
					justChanged_insidebands = false;
					insidebands = false;
					prev_insidebands = false;
					Qdelta = new Queue<double>();
					//bands = 0;
					prevValue = 0;
					prevValue_two = 0;
					bullish = false;
					bearish = false;
					prevBullish = false;
					prevBearish = false;

				}

				internal RMSobj(int period, int periodSmooth) : this()
				{
					IndPeriod = period;
					//sma = new SMAobj(period);
					SuperSmootherPeriod = 2 * periodSmooth;
				}

				// Initialize Bands
				internal void Init(double price)
				{
					Qdelta.Clear();

					//delta = close - alma;

					// initialise 3-Pole Super Smoother Variables
					a1 = Math.Exp(-3.1459 / SuperSmootherPeriod);
					b1 = 2 * a1 * Math.Cos(Math.PI * (1.738 * 180 / SuperSmootherPeriod) / 180.0);
					c1 = a1 * a1;
					coef2 = b1 + c1;
					coef3 = -(c1 + b1 * c1);
					coef4 = c1 * c1;
					coef1 = 1 - coef2 - coef3 - coef4;
					rmsSmooth = 0;
					rmsSmooth_Prev1 = 0;
					rmsSmooth_Prev2 = 0;
					rmsSmooth_Prev3 = 0;

					delta = 0;
					Qdelta.Enqueue(delta);
					sum = 0;

					prevValue_two = prevValue = price;
					for (int i = 0; i < IndPeriod; i++)
					{
						Qdelta.Enqueue(delta);
						sum += (delta * delta);
					}
					rms = 0;
				}

				//Mobo Calculation
				//internal double Calc(BarData b)

				internal double Calc(double price)
				{

					if (IndPeriod < 1)
						throw new Exception("Mobocalc: period < 1 invalid !!");

					if (firstRun)
					{
						Init(price);

						//Qdelta.Enqueue(b.close - b.ALMA);
						delta = price;

						Qdelta.Enqueue(delta);
						sum = 0;
						for (int i = 0; i < (IndPeriod - 1); i++)
						{
							Qdelta.Enqueue(delta);
							sum += (delta * delta);
							//sum += delta;
						}
						firstRun = false;

						//rms = Math.Sqrt(sum / (IndPeriod - 1));
						rms = Math.Sqrt((sum) / (IndPeriod - 1));
						//RMS = RMS + PB[count] * PB[count];
						//rms = sum / (IndPeriod - 1);

						prevValue_two = prevValue;
						prevValue = rms;

						return rms;

					}

					else  // !firstrun
					{

						//if (pos == 0 || pos == 1)
						//if (pos == 0)
						//{
						//	return rmsSmooth;
						//}

						//if (pos != 0 || pos != 1)
						//if (pos != 0)
						//{
						Qdelta.Enqueue(price);
						while (Qdelta.Count > (IndPeriod + 1))
						{
							Qdelta.Dequeue();
						}
						var arr = Qdelta.ToArray();
						var count = arr.Count();
						sum = 0;
						if (count > 1)
						{
							for (int i = 0; i < (IndPeriod - 1); i++)
							{
								sum += (arr[IndPeriod - i]) * (arr[IndPeriod - i]);
								//sum += arr[IndPeriod - i];
							}
						}

						//stdDev = Math.Sqrt(sum / (IndPeriod - 1));
						rms = Math.Sqrt((sum) / (IndPeriod - 1));
						//rms = sum / (IndPeriod - 1);

						rmsSmooth = coef1 * rms + coef2 * rmsSmooth_Prev1 + coef3 * rmsSmooth_Prev2 + coef4 * rmsSmooth_Prev3;
						rmsSmooth_Prev3 = rmsSmooth_Prev2;
						rmsSmooth_Prev2 = rmsSmooth_Prev1;
						rmsSmooth_Prev1 = rmsSmooth;
						//}
						return rmsSmooth;

					}

				}
			}

			FISHER_X ea;
			int counter;

			RMSobj RMSFast_Upper;
			RMSobj RMSFast_Lower;
			RMSobj RMSFast;
			ALMAobj SIGNAL;

			// Variables for Stoch
			int IndicatorPeriod;
			double value;
			Queue<double> Qprices;
			double highestValue;
			double lowestValue;
			double stochastic;
			double priceClose;
			double priceHigh;
			double priceOpen;
			double priceLow;
			double highestValue_Prev;
			double lowestValue_Prev;
			internal int SlopeThreshold;
			internal double thePrice;
			internal double prevStochastic;

			internal double fisher;
			internal double prevFisher;

			//Variables for HP Filt
			internal double alpha1;
			internal double HP;
			internal double HP_Prev1;
			internal double HP_Prev2;
			internal double thePrice_Prev1;
			internal double thePrice_Prev2;

			//Variables required for Price 2-pole and 3-Pole Super Smoother
			internal int SuperSmootherPeriod;
			internal double a1;
			internal double b1;
			internal double c1;
			internal double c2;
			internal double c3;
			internal double coef2;
			internal double coef3;
			internal double coef4;
			internal double coef1;
			internal double thePriceSmooth;
			internal double thePriceSmooth_Prev1;
			internal double thePriceSmooth_Prev2;
			internal double thePriceSmooth_Prev3;
			internal double HPPeriod;
			internal double HPSmooth;
			internal double HPSmooth_Prev1;
			internal double HPSmooth_Prev2;
			internal double HPSmooth_Prev3;

			//Variables required for Price Bandpass
			internal double BandWidth;
			internal double alpha2;
			internal double gamma1;
			internal double beta1;
			internal double Bandpass;
			internal double Bandpass_Prev1;
			internal double Bandpass_Prev2;

			//Variables required for Stochastic 3-Pole Super Smoother
			internal int SuperSmootherPeriod_Stoch;
			internal double a1_Stoch;
			internal double b1_Stoch;
			internal double c1_Stoch;
			internal double coef2_Stoch;
			internal double coef3_Stoch;
			internal double coef4_Stoch;
			internal double coef1_Stoch;
			internal double StochSmooth;
			internal double StochSmooth_Prev1;
			internal double StochSmooth_Prev2;
			internal double StochSmooth_Prev3;

			//Other Variables
			private bool firstRun;
			internal double SaturationLimit;
			internal bool bullsaturation;
			internal bool bearsaturation;
			internal double rms;
			internal double rms_upper;
			internal double rms_lower;
			internal double signal;

			internal FISHER_X_Obj()

			{
				Qprices = new Queue<double>();
				firstRun = true;
				SlopeThreshold = 10;
			}

			internal FISHER_X_Obj(FISHER_X eaIn, int period, int hpperiod, int periodSmooth, int periodSmooth_Stoch) : this ()
			{
				ea = eaIn;
				IndicatorPeriod = period;
				HPPeriod = hpperiod;
				SuperSmootherPeriod = periodSmooth;
				SuperSmootherPeriod_Stoch = periodSmooth_Stoch;

				RMSFast_Upper = new RMSobj(period*2, periodSmooth_Stoch);
				RMSFast_Lower = new RMSobj(period*2, periodSmooth_Stoch);
				RMSFast = new RMSobj(period*2, periodSmooth_Stoch);
				SIGNAL = new ALMAobj( (200), 20);
				//SaturationLimit = saturationLimit;
			}

			internal void Init(Bar theBar)

			{
				ea.Print("FISHER Init.");
				counter = 0;

				// initialise Stoch variables
				priceClose = (double)theBar.Close;
				priceHigh = (double)theBar.High;
				priceLow = (double)theBar.Low;
				thePrice = (priceClose + priceHigh) / 2;
				highestValue_Prev = 0;
				lowestValue_Prev = 0;
				prevStochastic = 0;

				RMSFast_Upper.Init(0);
				RMSFast_Lower.Init(0);
				RMSFast.Init(0);
				SIGNAL.Init(0);

				Qprices.Clear();
				for (int i = 0; i < IndicatorPeriod; i++)
				{
					Qprices.Enqueue(thePrice);
				}

				// initialise 2-Pole Price Super Smoother Variables
				a1 = Math.Exp(-1.414 * 3.1459 / SuperSmootherPeriod);
				b1 = 2 * a1 * Math.Cos(Math.PI * (1.414 * 180 / SuperSmootherPeriod) / 180.0);
				c2 = b1;
				c3 = -a1 * a1;
				c1 = 1 - c2 - c3;

				thePriceSmooth = 0;
				thePriceSmooth_Prev1 = 0;
				thePriceSmooth_Prev2 = 0;
				thePriceSmooth_Prev3 = 0;
				HPSmooth = 0;
				HPSmooth_Prev1 = 0;
				HPSmooth_Prev2 = 0;
				HPSmooth_Prev3 = 0;

				// initialise 3-Pole Stoch Super Smoother Variables
				a1_Stoch = Math.Exp(-3.1459 / SuperSmootherPeriod_Stoch);
				b1_Stoch = 2 * a1_Stoch * Math.Cos(Math.PI * (1.738 * 180 / SuperSmootherPeriod_Stoch) / 180.0);
				c1_Stoch = a1_Stoch * a1_Stoch;
				coef2_Stoch = b1_Stoch + c1_Stoch;
				coef3_Stoch = -(c1_Stoch + b1_Stoch * c1_Stoch);
				coef4_Stoch = c1_Stoch * c1_Stoch;
				coef1_Stoch = 1 - coef2_Stoch - coef3_Stoch - coef4_Stoch;
				StochSmooth = 0;
				StochSmooth_Prev1 = 0;
				StochSmooth_Prev2 = 0;
				StochSmooth_Prev3 = 0;

				// initialise HP Filter
				alpha1 = (Math.Cos(Math.PI * (0.707 * 360 / (HPPeriod)) / 180.0) + Math.Sin(Math.PI * (0.707 * 360 / (HPPeriod)) / 180.0) - 1) / (Math.Cos(Math.PI * (0.707 * 360 / (HPPeriod)) / 180.0));
				HP = 0;
				HP_Prev1 = 0;
				HP_Prev2 = 0;
				thePrice_Prev1 = 0;
				thePrice_Prev2 = 0;

				// initialise Bandpass filter
				//beta1 = (Math.Cos(Math.PI * (360 / (IndicatorPeriod)) / 180.0));
				//gamma1 = 1 / (Math.Cos(Math.PI * (360 * BandWidth / (IndicatorPeriod)) / 180.0));
				//alpha2 = gamma1 - Math.Sqrt(gamma1 * gamma1) - 1;

				rms = 0;
				rms_upper = 0;
				rms_lower = 0;
				signal = 0;

		}

			internal void Calc(Bar theBar)
			{

				//ea.Print(theBar.Close);
				if (theBar == null)
					throw new Exception("Stochastic.calc: theBar==null.");

				priceHigh = (double)theBar.High;
				priceClose = (double)theBar.Close;
				priceOpen = (double)theBar.Open;
				priceLow = (double)theBar.Low;

				if (firstRun)
				{
					Init(theBar);
					firstRun = false;
					for (int i = 0; i <= IndicatorPeriod; i++)
					{
						Qprices.Enqueue(thePrice);
					}
					//return thePrice;
				}

				else  // !firstrun
				{
					if (IndicatorPeriod < 1)
						throw new Exception("STOCH_EHLERcalc: period < 1 invalid !!");

				
						if (priceClose >= priceOpen)
						{
							thePrice = priceHigh;
						}

						if (priceClose < priceOpen)
						{
							thePrice = priceLow;
						}

						//HP Filter
						HP = (1 - alpha1 / 2) * (1 - alpha1 / 2) * (thePrice - 2 * thePrice_Prev1 + thePrice_Prev2) + 2 * (1 - alpha1) * HP_Prev1 - (1 - alpha1) * (1 - alpha1) * HP_Prev2;
						HP_Prev2 = HP_Prev1;
						HP_Prev1 = HP;
						thePrice_Prev2 = thePrice_Prev1;
						thePrice_Prev1 = thePrice;

						//Price Super Smoother
						HPSmooth = c1 * (HP + HP_Prev1) / 2 + c2 * HPSmooth_Prev1 + c3 * HPSmooth_Prev2;
						HPSmooth_Prev3 = HPSmooth_Prev2;
						HPSmooth_Prev2 = HPSmooth_Prev1;
						HPSmooth_Prev1 = HPSmooth;

						//Bandpass
						//Bandpass = 0.5 * (1 - alpha2) * (HP - HP_Prev1) + beta1 * (1 + alpha2) * Bandpass_Prev1 - alpha2 * HP_Prev2;
						//Bandpass_Prev2 = Bandpass_Prev1;
						//Bandpass_Prev1 = Bandpass;

						Qprices.Enqueue(HPSmooth);
						//Qprices.Enqueue(Bandpass);

						while (Qprices.Count > (IndicatorPeriod))
						{
							Qprices.Dequeue();
						}

						var arr = Qprices.ToArray();
						var count = arr.Count();

						highestValue = HPSmooth;
						lowestValue = HPSmooth;
						//highestValue = Bandpass;
						//lowestValue = Bandpass;

						if (count > 1)
						{
							for (int i = 0; i < (count - 1); i++)
							{
								if (arr[i] > highestValue)
								{
									highestValue = arr[i];
								}

								if (arr[i] < lowestValue)
								{
									lowestValue = arr[i];
								}
							}
						}

					//stochastic = (HPSmooth - lowestValue) / (highestValue - lowestValue);
					//stochastic = (Bandpass - lowestValue) / (highestValue - lowestValue);
					stochastic = 0.33 * 2*((HPSmooth - lowestValue) / (highestValue - lowestValue) - 0.5) + 0.67 * prevStochastic;
					prevStochastic = stochastic;

					if (stochastic > 0.99) stochastic = 0.999;
					if (stochastic < -0.99) stochastic = -0.999;

					fisher = 0.5 * Math.Log((1 + stochastic) / (1 - stochastic)) + 0.5 * prevFisher;

					prevFisher = fisher;

						//STOCHFast_Smooth_Period
						//STOCHSlow_Smooth_Period

						//lowestValue_Prev = lowestValue;
						//highestValue_Prev = highestValue;

						//double StochSmooth_Prev1;
						//double StochSmooth_Prev2;
						//double StochSmooth_Prev3;

					//Stoch Super Smoother
					StochSmooth = coef1_Stoch * fisher + coef2_Stoch * StochSmooth_Prev1 + coef3_Stoch * StochSmooth_Prev2 + coef4_Stoch * StochSmooth_Prev3;
					StochSmooth_Prev3 = StochSmooth_Prev2;
					StochSmooth_Prev2 = StochSmooth_Prev1;
					StochSmooth_Prev1 = StochSmooth;

					signal = SIGNAL.Calc(StochSmooth);

					rms = RMSFast.Calc(Math.Abs(StochSmooth-signal));

					if (StochSmooth >= signal)
					{
						rms_upper = RMSFast_Upper.Calc( (StochSmooth-signal) );
						RMSFast_Lower.Calc(0);
						rms_lower = 0;
					}
					if (StochSmooth < signal)
					{
						rms_lower = - RMSFast_Lower.Calc( (signal - StochSmooth) );
						RMSFast_Upper.Calc(0);
						rms_upper = 0;
					}

					//if (StochSmooth > 0.99999)
					//	StochSmooth = 0.99999;
					//if (StochSmooth < 0.00001)
					//	StochSmooth = 0.00001;

					//if (StochSmooth >= (1.0 - SaturationLimit) ) bullsaturation = true;
					//if (StochSmooth <= (0.0 + SaturationLimit) ) bearsaturation = true;

					//return StochSmooth;

				}

				counter++;

			}

		}

		private readonly Array<double> _FISHER_Up = new Array<double>();
		private readonly Array<double> _FISHER_Dn = new Array<double>();
		private readonly Array<double> _FISHER_Mid = new Array<double>();
		private readonly Array<double> _FISHER = new Array<double>();
		private readonly Array<double> _50 = new Array<double>();
		private readonly Array<double> _FISHER_Signal = new Array<double>();
		private readonly Array<double> _rmsUpper = new Array<double>();
		private readonly Array<double> _rmsLower = new Array<double>();

		private int draw_begin1;
		private int draw_begin2;
		int nBuffered;

		internal FISHER_X_Obj Fisher;          // internal Stoch indicator class object

		internal bool upperlimit;
		internal bool lowerlimit;

		int SlopeThreshold;

		public FISHER_X()

		{
			nBuffered = 0;
			indicator_chart_window = false;
			PriceType = PriceConstants.PRICE_CLOSE;

			indicator_buffers = 8;                          // 5 incator buffers for 3 lines on chart
			Fisher_Period = 6;								// default indicator Period
			Smooth_Period = 6;                            // default indicator Period
			Fisher_Smooth_Period = 1;                    // default indicator Period
			HP_Period = 30;                       // default indicator Period
			upperlimit = false;
			lowerlimit = false;

			ALMA_Period = 200;

			SlopeThreshold = 10;                            // default slopeThreshold

			Color transparentRed = Color.FromArgb(50, 255, 0, 0); //127 / 255 = 50 % of transparency
			Color transparentGreen = Color.FromArgb(50, 0, 128, 0); //55 / 255 = 50 % of transparency
			Color transparentWhite = Color.FromArgb(50, 255, 255, 255); //55 / 255 = 50 % of transparency
			Color transparentGolden = Color.FromArgb(70, 218, 165, 32); //55 / 255 = 50 % of transparency

			indicator_color1 = Colors.DeepSkyBlue;
			indicator_color2 = transparentGreen;
			indicator_color3 = transparentRed;
			indicator_color4 = transparentGolden;
			indicator_color5 = transparentWhite;
			indicator_color6 = Colors.Silver;
			indicator_color7 = transparentWhite;
			indicator_color8 = transparentWhite;

			SetIndexLabel(0, string.Format("FisherX({0})", Fisher_Period));
			SetIndexLabel(1, "High");
			SetIndexLabel(2, "Low");
			SetIndexLabel(3, "Mid");
			SetIndexLabel(4, "50");
			SetIndexLabel(5, "Signal");
			SetIndexLabel(6, "RMSUpper");
			SetIndexLabel(7, "RMSLower");

			IndicatorShortName(string.Format("FISHER_X({0})", Fisher_Period));

		}

		#region UserSettings

		[Description("Period of the Fisher Indicator in Bars. [ex: 6]")]
		[Category("Fisher Settings")]
		[DisplayName("Fisher Normalising Period")]
		public int Fisher_Period { get; set; }

		[Description("Period of the High Pass filter for Fast Stoch in Bars. [ex: 30]")]
		[Category("Fisher Settings")]
		[DisplayName("HP Filter Period")]
		public int HP_Period { get; set; }

		[Description("Period of the Smoother for Price used in the Slow Stoch Indicator in Bars. [ex: 6]")]
		[Category("Fisher Settings")]
		[DisplayName("Price Smoother Period")]
		public int Smooth_Period { get; set; }

		[Description("Period of the Smoother for Price used in the Fast Stoch Indicator in Bars. [ex: 1]")]
		[Category("Fisher Settings")]
		[DisplayName("Fisher Smoother Period")]
		public int Fisher_Smooth_Period { get; set; }

		[Description("Period of the ALMA Centre Line in Bars. [ex: 200]")]
		[Category("RMS Settings")]
		[DisplayName("Centre Line Moving Avg Period")]
		public int ALMA_Period { get; set; }








		#endregion

		public PriceConstants PriceType { get; set; }

		protected override int Init()

		{
			nBuffered = 0;
			Fisher = new FISHER_X_Obj(this, Fisher_Period, HP_Period, Smooth_Period, Fisher_Smooth_Period);    // create Slow STOCH indicator object

			string short_name;

			SetIndexBuffer(0, _FISHER);
			SetIndexBuffer(1, _FISHER_Up);
			SetIndexBuffer(2, _FISHER_Dn);
			SetIndexBuffer(3, _FISHER_Mid);
			SetIndexBuffer(4, _50);
			SetIndexBuffer(5, _FISHER_Signal);
			SetIndexBuffer(6, _rmsUpper);
			SetIndexBuffer(7, _rmsLower);

			SetIndexStyle(0, DRAW_LINE, 0, 2, indicator_color1);
			SetIndexStyle(1, DRAW_HISTOGRAM, 0, 3, indicator_color2);
			SetIndexStyle(2, DRAW_HISTOGRAM, 0, 3, indicator_color3);
			SetIndexStyle(3, DRAW_HISTOGRAM, 0, 3, indicator_color4);
			SetIndexStyle(4, DRAW_LINE, 0, 1, indicator_color5);
			SetIndexStyle(5, DRAW_LINE, 0, 1, indicator_color6);
			SetIndexStyle(6, DRAW_LINE, 0, 2, indicator_color7);
			SetIndexStyle(7, DRAW_LINE, 0, 2, indicator_color8);

			ArrayInitialize(_FISHER, 0.00);
			ArrayInitialize(_FISHER_Up, EMPTY_VALUE);
			ArrayInitialize(_FISHER_Dn, EMPTY_VALUE);
			ArrayInitialize(_FISHER_Mid, EMPTY_VALUE);
			ArrayInitialize(_50, 0.0);
			ArrayInitialize(_FISHER_Signal, 0.00);
			ArrayInitialize(_rmsUpper, 0.00);
			ArrayInitialize(_rmsLower, 0.00);

			SetIndexDrawBegin(0, HP_Period * 4);
			SetIndexDrawBegin(1, HP_Period * 4);
			SetIndexDrawBegin(2, HP_Period * 4);
			SetIndexDrawBegin(3, HP_Period * 4);
			SetIndexDrawBegin(4, HP_Period * 4);
			SetIndexDrawBegin(5, HP_Period * 4);
			SetIndexDrawBegin(6, HP_Period * 4);
			SetIndexDrawBegin(7, HP_Period * 4);

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

					Fisher.Init(theBar);

					for (i = 0; i < cntB; i++)
					{
						int pos = cntB - i - 1;
						theBar = data[pos];

						Fisher.Calc(theBar);

						_FISHER[pos] = Fisher.StochSmooth;

						uptrend[pos] = uptrend[pos + 1];

						if (_FISHER[pos] > _FISHER[pos + 1]) uptrend[pos] = 1;

						if (_FISHER[pos] < _FISHER[pos + 1]) uptrend[pos] = -1;

						_FISHER_Signal[pos] = Fisher.signal;

						_rmsUpper[pos] = _FISHER_Signal[pos] + Fisher.rms;
						_rmsLower[pos] = _FISHER_Signal[pos] - Fisher.rms;

						if ((uptrend[pos] == 1))
						{
							_FISHER_Up[pos] = _FISHER[pos];
							_FISHER_Dn[pos] = EMPTY_VALUE;
						}
						if (upperlimit)
						{
							_FISHER_Up[pos] = _FISHER[pos];
							_FISHER_Dn[pos] = EMPTY_VALUE;
						}
						if ((uptrend[pos] == -1))
						{
							_FISHER_Dn[pos] = _FISHER[pos];
							_FISHER_Up[pos] = EMPTY_VALUE;
						}
						if ((lowerlimit))
						{
							_FISHER_Dn[pos] = _FISHER[pos];
							_FISHER_Up[pos] = EMPTY_VALUE;
						}

						_50[pos] = 0.00;
						

						nBuffered++;
					}
				}

				else
				{
					
					var cnt = Bars - counted_bars;
					for (i = 0; i < cnt; i++)
					{
						int pos2 = cnt - i - 1;
						theBar = data[pos2];

						Fisher.Calc(theBar);

						_FISHER[pos2] = Fisher.StochSmooth;

						uptrend[pos2] = uptrend[pos2 + 1];
						if (_FISHER[pos2] > _FISHER[pos2 + 1]) uptrend[pos2] = 1;
						if (_FISHER[pos2] < _FISHER[pos2 + 1]) uptrend[pos2] = -1;

						if (_FISHER[pos2] == 0.99999) upperlimit = true;
						else upperlimit = false;

						if (_FISHER[pos2] == 0.00001) lowerlimit = true;
						else lowerlimit = false;

						if ((uptrend[pos2] == 1) || upperlimit)
						{
							_FISHER_Up[pos2] = _FISHER[pos2];
							_FISHER_Dn[pos2] = EMPTY_VALUE;
						}

						if ((uptrend[pos2] == -1) || lowerlimit)
						{
							_FISHER_Dn[pos2] = _FISHER[pos2];
							_FISHER_Up[pos2] = EMPTY_VALUE;
						}

						_50[pos2] = 0.00;
						_FISHER_Signal[pos2] = Fisher.signal;

						_rmsUpper[pos2] = _FISHER_Signal[pos2] + Fisher.rms;
						_rmsLower[pos2] = _FISHER_Signal[pos2] - Fisher.rms;

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