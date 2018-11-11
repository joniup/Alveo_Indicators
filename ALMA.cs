
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
    public class ALMA : IndicatorBase
    {
        #region Properties

        // User settable Properties for this Alveo Indicator
        /// <param name="period">Sets the strength of the filtering.</param>
        [Category("Settings")]
        [DisplayName("MA Period. ")]
        [Description("Sets the strength of the filtering. [ex: 20]")]
        public int IndPeriod { get; set; }

        [Category("Settings")]
        [DisplayName("ALMA Offset. ")]
        [Description("ALMA Offset (look back). [ex: 0.15]")]
        public double Offset { get; set; }

        /// <param name="slopeThreshold">Specifies at what slope Uptrend and Downtrend are determined.</param>
        [Category("Settings")]
        [DisplayName("Slope Trheshold * 1e-6. ")]
        [Description("Specifies at what slope Uptrend and Downtrend are determined. [ex: 10]")]
        public int SlopeThreshold { get; set; }
        #endregion

        //Buffers for Alveo DTEMA Indicator
        Array<double> UpTrend;          // upwards trend
        Array<double> DownTrend;        // downwards trend
        Array<double> Consolidation;    // in between UpTrend and DownTrend

        Bar b;              // holds latest chart Bar data from Alveo
        Bar prevBar;        // holds latest chart Bar data from Alveo
        int counted_bars;   // amount of bars of bars on chart already processed by the indicator.
        int e;              // number of bars for the indicator to calculate
        double thePrice;    // holds the currency pair price for the NDI calculation. In units of the bas currency.
        bool firstrun;      // firstrun = true on first execution of the Start function. False otherwise.
        
        //DTEMAobj DTema;
        ALMAobj alma;

        const string dataFileDir = "C:\\temp\\ALMA\\";

        /// <summary>  
        ///  C# constructor for ALMA Class
        ///  called to initialize the class when class is created by Alveo
        /// </summary>  
        public ALMA()
        {
            try
            {
                // Basic indicator initialization. Don't use this constructor to calculate values

                indicator_buffers = 3;              // 3 lines on Alveo chart require 3 buffers
                indicator_chart_window = true;
                firstrun = true;                    // initially set true for first run
                prevBar = null;

                IndPeriod = 35;                     // Initial value for DTEMAv2 period
                SlopeThreshold = 10;                // Initial value for slopeThreshold
                Offset = 0.15;

                indicator_width1 = 3;               // width of line 1 on the chart
                indicator_width2 = 3;
                indicator_width3 = 3;
                indicator_color1 = Colors.MediumBlue;     // line colors
                indicator_color2 = Colors.Aqua;
                indicator_color3 = Colors.Magenta;

                UpTrend = new Array<double>();      // 3 data buffers for Alveo Indicator
                DownTrend = new Array<double>();
                Consolidation = new Array<double>();

                copyright = "";
                link = "";
            }
            catch (Exception ex)  // Print don't function at this point in Alveo
            {
                Print("ALMA: Exception: " + ex.Message);
                Print("ALMA: " + ex.StackTrace);
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
                //SetIndexLabel(0, "DTEMA(" + IndPeriod + ").Bull");   // Sets description for showing in the DataWindow and in the tooltip on Chart.
                SetIndexLabel(0, "ALMA(" + IndPeriod + ").Bull");

                SetIndexStyle(1, DRAW_LINE, STYLE_SOLID);       // repeat for all 3 buffers
                //SetIndexLabel(1, "DTEMA(" + IndPeriod + ").Bear");
                SetIndexLabel(1, "ALMA(" + IndPeriod + ").Bear");

                SetIndexStyle(2, DRAW_LINE, STYLE_SOLID);
                //SetIndexLabel(2, "DTEMA(" + IndPeriod + ").Mixed");
                SetIndexLabel(2, "ALMA(" + IndPeriod + ").Mixed");

                // Sets the "short" name of a custom indicator to be shown in the DataWindow and in the chart subwindow.
                //IndicatorShortName("DTEMA v2.0 (" + IndPeriod + "," + SlopeThreshold + ")");
                IndicatorShortName("ALMA (" + IndPeriod + "," + SlopeThreshold + ")");

                //DTema = new DTEMAobj(IndPeriod, SlopeThreshold);
                alma = new ALMAobj(IndPeriod, SlopeThreshold, Offset);

                Print("ALMA: Started. [" + Chart.Symbol + "] tf=" + Period());      // Print this message to Alveo Log file on startup
            }
            catch (Exception ex)
            {
                Print("ALMA: Init: Exception: " + ex.Message);
                Print("ALMA: " + ex.StackTrace);
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

                if (firstrun)   // on first run, calculate ALMA on all ChartBars data
                {
                    b = ChartBars[e - 1];           // b = refernec to oldest ChartBars data
                    prevBar = b;
                    thePrice = (double)(b.Open + b.Close) / 2.0;     // initialize DTEMA to thePrice of the oldest bar
                    
                    alma.Init(thePrice);

                    firstrun = false;               // firstrun initialization completed 
                }
                else // not firstrun; only calculate DTEMA on ChartBars not already processed
                {
                    counted_bars = IndicatorCounted();
                    if (counted_bars < 0)
                        throw new Exception("ALMA: Start: invalid IndicatorCounted value.");            // invalid value
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
                        Print("ALMA: Gap detected. gap=" + gap / Point + " points. " + b.BarTime.ToString());
                        
                        alma.Init(thePrice);
                    }
                    
                    alma.Calc(thePrice);                                        // calculate new ALMA value

                    UpdateBuffers(i);                                           // update Alvelo Indicator buffers
                }
            }
            catch (Exception ex)    // catch and Print any exceptions that may have happened
            {
                Print("ALMA: Start: Exception: " + ex.Message);
                Print("ALMA: " + ex.StackTrace);
            }
            return 0;
        }

        /// <summary>  
        ///  UpdateBuffers - update Alveo Indicator buffers with new data
        /// <param name="entry">index value into buffer.</param>
        /// </summary>
        
        internal void UpdateBuffers(int entry)
        {
            //var DTEMAvalue = DTema.DTEMAval;
            var alma_value = alma.value;

            var indx = e - entry - 1;               // put data into buffers in the reverse order, for Alveo.
            UpTrend[indx] = EMPTY_VALUE;            // Initialize with EMPTY_VALUE
            DownTrend[indx] = EMPTY_VALUE;
            Consolidation[indx] = EMPTY_VALUE;
            
            if (alma.isRising)   // if UpTrend
            {
                UpTrend[indx] = alma_value;             // UpTrend buffer gets ALMAvalue
            }
            else if (alma.isFalling) // Downtrend
            {
                DownTrend[indx] = alma_value;           // DownTrend buffer gets ALMAvalue
            }
            else  // otherwise
            {
                Consolidation[indx] = alma_value;       // Consolidation buffer gets ALMAvalue
            }

            if (alma.justChangedDir)                    // if trendDir changed from previous call
            {
                switch (alma.prevState)             // place connecting ALMA into proper buffer to connect the lines
                {
                    case 1: // uptrend
                        UpTrend[indx] = alma_value;
                        break;
                    case -1: // downtrend
                        DownTrend[indx] = alma_value;
                        break;
                    case 0:  // consolidation
                        Consolidation[indx] = alma_value;
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
            internal double OFFset;

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
            internal ALMAobj(double period, int threshold, double offset) : this()
            {
                Period = period;
                OFFset = offset;
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
                double m = Math.Floor((1+OFFset) * (Period));
                double s = Period / 6.0;
                alma = 0;
                wSum = 0;
                double w = 0;
                int Period_int = (int)Period;

                if (Period < 1)
                    throw new Exception("Almacalc: period < 1 invalid !!");
                
                value = 0;
                Qprices.Enqueue(Price);
                while (Qprices.Count > (Period + 1))
                {
                    Qprices.Dequeue();
                }

                var arr = Qprices.ToArray();
                var count = arr.Count();

                if (count > 1)
                {
                    for (int i = 0; i < (count - 1); i++)
                    {
                        w = Math.Exp(-(i - m) * (i - m) / (2 * s * s));
                        alma += arr[i] * w;
                        wSum += w;
                    }
                }

                value = alma / wSum;

                //price direction
                velocity = value - prevValue;
                //acceleration = velocity - (prevValue_two - prevValue_three);

                //prev_acceleration = acceleration;

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

                prevValue_three = prevValue_two;
                prevValue_two = prevValue;
                prevValue = value;

                return value;
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