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
	[Description("Trend Channel Indicator")]

	public class FTR_BANDS : IndicatorBase
	{

		internal class FTR_BANDS_Obj
		{
			/*
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
						/*

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
			*/

			//atr indicator
			internal class FTRobj
			{
				//internal SMAobj sma;
				internal int IndPeriod;
				internal bool firstRun;
				internal double TR;
				internal double value;
				internal double prev_value;
				internal double prev_value2;
				internal Bar prevBar;

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
				internal double trSmooth_Prev2;
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

				internal double priceHigh;
				internal double priceLow;
				internal double priceClose;
				internal double priceOpen;
				internal double thePrice;
				internal double thePrice_Prev;

				//Variables Required for Decycler
				internal double alpha1;
				internal int Cutoff;
				internal double PriceDecycle;
				internal double PriceDecycle_Prev1;

				//const string dataFileDir = "C:\\temp\\";
				//System.IO.StreamWriter dfile = null;

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

			FTR_BANDS ea;

			Decycler_obj priceDecycle;
			SUPERSMOOTHER_3Pole_obj priceDecycleSmooth;
			SUPERSMOOTHER_3Pole_obj priceSmooth;
			//ALMAobj deviationMean;
			FTRobj filtertruerange;
			Decycler_obj ftrDecycle;

			internal double Offset_Factor;
			internal double Min_Offset;

			internal Bar prevBar;        
			internal int counted_bars;  
			internal int e;
			internal int counter;

			internal bool firstrun;      
			internal double bandOffset;
			internal double prev_max_AbsDifference;
			internal double max_AbsDifference;
			internal double Deviation;
			internal double DeviationAbs;
			internal double MeanDeviation;
			internal double upperband;
			internal double lowerband;

			internal bool channel_isRising;
			internal bool channel_isFalling;
			internal double Channel_CentreLine;
			internal int trendDir;
			internal int prevTrendDir;
			internal int prevState;
			internal bool trendChanged;
			internal bool stateChanged;

			internal double thePrice;
			internal double priceClose;
			internal double priceOpen;
			internal double priceAvg;

			internal FTR_BANDS_Obj()

			{
				priceClose = 0;
				priceOpen = 0;
				priceAvg = 0;
				thePrice = 0;

				Offset_Factor = 0;
				Min_Offset = 0;

				channel_isRising = false;
				channel_isFalling = false;
				trendDir = 0;
				prevTrendDir = 0;
				prevState = 0;
				trendChanged = false;
				stateChanged = false;

				firstrun = false;
				bandOffset = 0;
				prev_max_AbsDifference = 0;
				max_AbsDifference = 0;
				Deviation = 0;
				DeviationAbs = 0;
				MeanDeviation = 0;
				upperband = 0;
				lowerband = 0;
				Channel_CentreLine = 0;

				counter = 0;
		}

			//trendchannel = new TREND_CHANNEL_Obj(this, IndPeriod, CutoffPeriod, Offset_Factor, Min_Offset, SlopeThreshold);
			internal FTR_BANDS_Obj(FTR_BANDS eaIn, int indperiod, int cutoffperiod, double offset_factor, double min_offset, int slopethreshold) : this()
			{
				ea = eaIn;
				//deviationMean = new ALMAobj(cutoffperiod, slopethreshold);
				priceDecycle = new Decycler_obj(indperiod, slopethreshold, cutoffperiod);
				ftrDecycle = new Decycler_obj(2, slopethreshold, cutoffperiod);
				priceDecycleSmooth = new SUPERSMOOTHER_3Pole_obj(indperiod, slopethreshold);
				priceSmooth = new SUPERSMOOTHER_3Pole_obj(indperiod, slopethreshold);
				filtertruerange = new FTRobj(cutoffperiod);
				Offset_Factor = offset_factor;
				Min_Offset = min_offset;
			}

			internal void Init(Bar theBar)

			{
				ea.Print("MACD2 Init.");
				counter = 0;

				priceClose = (double)theBar.Close;
				priceOpen = (double)theBar.Open;
				priceAvg = (priceClose + priceOpen) / 2;

				priceDecycleSmooth.Init(thePrice);
				priceDecycle.Init(thePrice);
				//deviationMean.Init(0);
				priceSmooth.Init(thePrice);
				filtertruerange.Init(theBar);

				priceClose = 0;
				priceOpen = 0;
				priceAvg = 0;
				thePrice = 0;

				channel_isRising = false;
				channel_isFalling = false;
				trendDir = 0;
				prevTrendDir = 0;
				prevState = 0;
				trendChanged = false;
				stateChanged = false;

				firstrun = false;
				bandOffset = 0;
				prev_max_AbsDifference = 0;
				max_AbsDifference = 0;
				Deviation = 0;
				DeviationAbs = 0;
				MeanDeviation = 0;

				upperband = 0;
				lowerband = 0;
				Channel_CentreLine = 0;

			}

			internal void Calc(Bar theBar)
			{
				//ea.Print(theBar.Close);
				if (theBar == null)
					throw new Exception("Stochastic.calc: theBar==null.");
				priceClose = (double)theBar.Close;
				priceOpen = (double)theBar.Open;
				priceAvg = (priceClose + priceOpen) / 2;

				//ftr = FTR.Calc(theBar);
				//ftr_alma = FTRDecycle.Calc(ftr);

				priceSmooth.Calc(((double)theBar.Open + (double)theBar.Low + (double)theBar.High + (double)theBar.Close) / 4);
				filtertruerange.Calc(theBar);
				priceDecycle.Calc(priceSmooth.PriceSmooth);
				ftrDecycle.Calc(filtertruerange.value);
				Channel_CentreLine = priceDecycleSmooth.Calc(priceDecycle.PriceDecycle);

				//Deviation = priceSmooth.PriceSmooth - priceDecycleSmooth.PriceSmooth;
				//DeviationAbs = Math.Abs(Deviation);
				//MeanDeviation = deviationMean.Calc(DeviationAbs);
				//bandOffset = Offset_Factor * MeanDeviation;
				bandOffset = Offset_Factor * ftrDecycle.PriceSmooth;

				//upperband = priceDecycleSmooth.PriceSmooth + Math.Max(bandOffset, Min_Offset);
				upperband = priceDecycleSmooth.PriceSmooth + bandOffset;
				//lowerband = priceDecycleSmooth.PriceSmooth - Math.Max(bandOffset, Min_Offset);
				lowerband = priceDecycleSmooth.PriceSmooth - bandOffset;

				//indicator state
				channel_isRising = priceDecycleSmooth.isRising;
				channel_isFalling = priceDecycleSmooth.isFalling;
				if (trendDir != 0)
					prevTrendDir = trendDir;
				prevState = trendDir;
				trendDir = channel_isRising ? 1 : (channel_isFalling ? -1 : 0);
				trendChanged = (trendDir * prevTrendDir < 0);
				stateChanged = (trendDir != prevState);

				counter++;

			}

		}

		Array<double> _UpTrend = new Array<double>();          
		Array<double> _DownTrend = new Array<double>();        
		Array<double> _Consolidation = new Array<double>();    
		Array<double> _Upper = new Array<double>();
		Array<double> _Lower = new Array<double>();
		Array<double> _EXTREME_BEAR = new Array<double>();
		Array<double> _EXTREME_BULL = new Array<double>();

		const string dataFileDir = "C:\\temp\\ALMA\\";

		private int draw_begin1;
		private int draw_begin2;
		int nBuffered;
		FTR_BANDS_Obj ftrband;
		private double tchannel;

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
		double markeroffset;

		public FTR_BANDS()

		{
			nBuffered = 0;
			indicator_buffers = 7;
			indicator_chart_window = true;
			PriceType = PriceConstants.PRICE_CLOSE;
			Marker_Offset_Pips = 5;
			show_PRICE_EXTREMES = true;


			IndPeriod = 10;               
			SlopeThreshold = 10;                
			Offset_Factor = 3;
			//Min_Offset = 35;
			bandOffset = 0;
			Deviation = 0;
			MeanDeviation = 0;
			prev_max_AbsDifference = 0.0;
			max_AbsDifference = 0.0;
			CutoffPeriod = 20;
			markeroffset = 0;

			Color transparentRed = Color.FromArgb(50, 255, 0, 0); //127 / 255 = 50 % of transparency
			Color transparentGreen = Color.FromArgb(50, 0, 128, 0); //55 / 255 = 50 % of transparency
			Color transparentGolden = Color.FromArgb(50, 218, 165, 32); //55 / 255 = 50 % of transparency
			Color transparentWhite = Color.FromArgb(80, 255, 255, 255); //55 / 255 = 50 % of transparency

			indicator_color1 = transparentWhite;
			indicator_color2 = transparentWhite;
			indicator_color3 = transparentWhite;
			indicator_color4 = transparentWhite;
			indicator_color5 = transparentWhite;
			indicator_color6 = transparentGolden;
			indicator_color7 = transparentGolden;

			var short_name = "FTR_BANDS(" + CutoffPeriod + "," + Offset_Factor + ")";
			IndicatorShortName(short_name);
			SetIndexLabel(0, "T_Channel(" + CutoffPeriod + ").Bull");
			SetIndexLabel(1, "T_Channel(" + CutoffPeriod + ").Bear");
			SetIndexLabel(2, "T_Channel(" + CutoffPeriod + ").Mixed");
			SetIndexLabel(3, "T_Channel(" + CutoffPeriod + ").Upper");
			SetIndexLabel(4, "T_Channel(" + CutoffPeriod + ").Lower");
			SetIndexLabel(5, "Extreme Bull");
			SetIndexLabel(6, "Extreme Bear");
		}

		#region UserSettings

		[Category("Ftr Settings")]
		[DisplayName("Smoother Period. ")]
		[Description("Sets the strength of the smoother filter. [ex: 20]")]
		public int IndPeriod { get; set; }

		[Category("Ftr Settings")]
		[DisplayName("Cutoff Period. ")]
		[Description("Sets the cutoff period for the Decycler (centre line). [ex: 200]")]
		public int CutoffPeriod { get; set; }

		[Category("Ftr Settings")]
		[DisplayName("Band offset Factor. ")]
		[Description("Factor used to multiply the average distance from the centreline. [ex: 2]")]
		public double Offset_Factor { get; set; }

		//[Category("Ftr Settings")]
		//[DisplayName("Min Band Offset (pips). ")]
		//[Description("Minimum band offset pips. [ex: 40]")]
		//public double Min_Offset { get; set; }

		[Category("Ftr Settings")]
		[DisplayName("Slope Trheshold * 1e-6. ")]
		[Description("Specifies at what slope Uptrend and Downtrend are determined. [ex: 10]")]
		public int SlopeThreshold { get; set; }

		[Category("Display Settings")]
		[DisplayName("Select to show extreme price outside of bands")]
		public bool show_PRICE_EXTREMES{ get; set; }

		[Category("Display Settings")]
		[DisplayName("Distance of arrows to candles - in Pips")]
		public double Marker_Offset_Pips { get; set; }

		#endregion

		public PriceConstants PriceType { get; set; }

		protected override int Init()

		{
			nBuffered = 0;

			//double Min_Offset_Adjusted;
			string currentSymbol = Symbol();

			if (currentSymbol.StartsWith("JPY") || currentSymbol.EndsWith("JPY")) markeroffset = Marker_Offset_Pips / 100;
			else if (currentSymbol.StartsWith("XAU") || currentSymbol.EndsWith("XAU")) markeroffset = Marker_Offset_Pips / 10;
			else if (currentSymbol.StartsWith("XAG") || currentSymbol.EndsWith("XAG")) markeroffset = Marker_Offset_Pips / 1000;
			else markeroffset = Marker_Offset_Pips / 10000;

			ftrband = new FTR_BANDS_Obj(this, IndPeriod, CutoffPeriod, Offset_Factor, markeroffset, SlopeThreshold);

			SetIndexBuffer(0, _UpTrend);
			SetIndexBuffer(1, _DownTrend);
			SetIndexBuffer(2, _Consolidation);
			SetIndexBuffer(3, _Upper);
			SetIndexBuffer(4, _Lower);
			SetIndexBuffer(5, _EXTREME_BEAR);
			SetIndexBuffer(6, _EXTREME_BULL);

			SetIndexStyle(0, DRAW_LINE, STYLE_SOLID);
			SetIndexStyle(1, DRAW_LINE, STYLE_SOLID);
			SetIndexStyle(2, DRAW_LINE, STYLE_SOLID);
			SetIndexStyle(3, DRAW_LINE, STYLE_SOLID);
			SetIndexStyle(4, DRAW_LINE, STYLE_SOLID);
			SetIndexStyle(5, DRAW_ARROW);
			SetIndexStyle(6, DRAW_ARROW);

			SetIndexArrow(5, 241);
			SetIndexArrow(6, 242);

			ArrayInitialize(_UpTrend, 0.0);
			ArrayInitialize(_DownTrend, EMPTY_VALUE);
			ArrayInitialize(_Consolidation, EMPTY_VALUE);
			ArrayInitialize(_Upper, 0.0);
			ArrayInitialize(_Lower, 0.0);
			ArrayInitialize(_EXTREME_BEAR, EMPTY_VALUE);
			ArrayInitialize(_EXTREME_BULL, EMPTY_VALUE);

			SetIndexDrawBegin(0, IndPeriod);
			SetIndexDrawBegin(1, IndPeriod);
			SetIndexDrawBegin(2, IndPeriod);
			SetIndexDrawBegin(3, IndPeriod);
			SetIndexDrawBegin(4, IndPeriod);
			SetIndexDrawBegin(5, IndPeriod);
			SetIndexDrawBegin(6, IndPeriod);

			SetIndexLabel(0, "T_Channel(" + CutoffPeriod + ").Bull");      
			SetIndexLabel(1, "T_Channel(" + CutoffPeriod + ").Bear");
			SetIndexLabel(2, "T_Channel(" + CutoffPeriod + ").Mixed");   
			SetIndexLabel(3, "T_Channel(" + CutoffPeriod + ").Upper");    
			SetIndexLabel(4, "T_Channel(" + CutoffPeriod + ").Lower");
			SetIndexLabel(5, "Extreme Bull");
			SetIndexLabel(6, "Extreme Bear");

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
					ftrband.Init(theBar);
					for (i = 0; i < cntB; i++)
					{
						int pos = cntB - i - 1;
						theBar = data[cntB - i - 1];

						_UpTrend[pos] = EMPTY_VALUE;
						_DownTrend[pos] = EMPTY_VALUE;
						_Consolidation[pos] = EMPTY_VALUE;
						_Upper[pos] = EMPTY_VALUE;
						_Lower[pos] = EMPTY_VALUE;
						_EXTREME_BEAR[pos] = EMPTY_VALUE;
						_EXTREME_BULL[pos] = EMPTY_VALUE;

						ftrband.Calc(theBar);

						_Upper[pos] = ftrband.upperband;
						_Lower[pos] = ftrband.lowerband;

						if (ftrband.channel_isRising)   // if UpTrend
						{
							_UpTrend[pos] = ftrband.Channel_CentreLine;             // UpTrend buffer gets ALMAvalue
						}
						else if (ftrband.channel_isFalling) // Downtrend
						{
							_DownTrend[pos] = ftrband.Channel_CentreLine;           // DownTrend buffer gets ALMAvalue
						}
						else  // otherwise
						{
							_Consolidation[pos] = ftrband.Channel_CentreLine;       // Consolidation buffer gets ALMAvalue
						}

						if (ftrband.trendChanged)                    // if trendDir changed from previous call
						{
							switch (ftrband.prevState)             // place connecting ALMA into proper buffer to connect the lines
							{
								case 1: // uptrend
									_UpTrend[pos] = ftrband.Channel_CentreLine;
									break;
								case -1: // downtrend
									_DownTrend[pos] = ftrband.Channel_CentreLine;
									break;
								case 0:  // consolidation
									_Consolidation[pos] = ftrband.Channel_CentreLine;
									break;
							}
						}

						

						if (show_PRICE_EXTREMES)
						{
							if ((double)theBar.High >= _Upper[pos])
							{
								_EXTREME_BULL[pos] = (double)theBar.High + markeroffset;
							}
							if ((double)theBar.Low <= _Lower[pos])
							{
								_EXTREME_BEAR[pos] = (double)theBar.Low - markeroffset;
							}

						}

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

						ftrband.Calc(theBar);

						_UpTrend[pos2] = EMPTY_VALUE;
						_DownTrend[pos2] = EMPTY_VALUE;
						_Consolidation[pos2] = EMPTY_VALUE;
						_Upper[pos2] = EMPTY_VALUE;
						_Lower[pos2] = EMPTY_VALUE;

						_Upper[pos2] = ftrband.upperband;
						_Lower[pos2] = ftrband.lowerband;

						if (ftrband.channel_isRising)   // if UpTrend
						{
							_UpTrend[pos2] = ftrband.Channel_CentreLine;             // UpTrend buffer gets ALMAvalue
						}
						else if (ftrband.channel_isFalling) // Downtrend
						{
							_DownTrend[pos2] = ftrband.Channel_CentreLine;           // DownTrend buffer gets ALMAvalue
						}
						else  // otherwise
						{
							_Consolidation[pos2] = ftrband.Channel_CentreLine;       // Consolidation buffer gets ALMAvalue
						}

						if (ftrband.trendChanged)                    // if trendDir changed from previous call
						{
							switch (ftrband.prevState)             // place connecting ALMA into proper buffer to connect the lines
							{
								case 1: // uptrend
									_UpTrend[pos2] = ftrband.Channel_CentreLine;
									break;
								case -1: // downtrend
									_DownTrend[pos2] = ftrband.Channel_CentreLine;
									break;
								case 0:  // consolidation
									_Consolidation[pos2] = ftrband.Channel_CentreLine;
									break;
							}
						}

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