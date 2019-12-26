#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// 
    /// SMA 200 Crossover
    /// 
    /// </summary>
	[Description("Ninjatrader Stock Investment Strategy for Backtesting - " +
    "I buy the stock when its price crosses above the 200-days moving average from below " +
    "and sell it when its price crosses below the moving average from above.")]
  
    public class CustomStrategySma200K : Strategy
    {
        #region Variables

        // User defined variables (add any user defined variables below)

        private int smaFast = 1; // Default setting
        private int smaSlow = 200; // Default setting        
        private int crossBarsBack = 1; // SMA Cross Above oder Cross Below within the previous n bars defined in crossBarsBack
		
        private double accountSize = 25000; // Default setting
        private int positionSize = 0; // Default setting
        private double val = 0; // Default setting
		private double entryPrice = 0; // Default setting
        private double exitPrice = 0; // Default setting
        private double profitLoss = 0; // Default setting
	
		private bool isInvested = false; // Default setting

        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called
        /// </summary>
        protected override void Initialize()
        {
			// Add simple moving averages to the chart for display
			// This only displays the SMA's for the primary Bars object (index = 0) on the chart
			Add(SMA(SmaFast));
			Add(SMA(SmaSlow));
			SMA(SmaFast).Plots[0].Pen.Color = Color.Green;
			SMA(SmaSlow).Plots[0].Pen.Color = Color.Violet;

			// Strategy starts if we have enough bars
			BarsRequired = 200;

			// CalculateOnBarClose is set to true and sets the strategy to call the OnBarUpdate() method
			// below on the close of each bar instead of each incoming tick.
			CalculateOnBarClose = true;
        }

        /// <summary>
        /// SMA Crossover - Called on each bar update event (incoming tick)
		/// </summary>
        protected override void OnBarUpdate()
        {
            // Long signal
            // Go long if the fast SMA crosses above slow SMA within the previous n bars in crossBarsBack
			if (!isInvested)
			{
				if (CrossAbove(SMA(SmaFast), SMA(SmaSlow), CrossBarsBack))
				{
                    entryPrice = Open[-1]; // Take open price next day - [0] Current Bar Value, [1] Previous Bar Values, [-1] Next Bar Value
                    positionSize = Convert.ToInt32(accountSize / entryPrice);
					
					EnterLong(positionSize, "Long");
				
					isInvested = true;
				}
			}
			// Exit signal
			// Exit long if fast SMA crosses below slow SMA within the previous n bars in crossBarsBack
			else if (isInvested)
			{
				if (CrossBelow(SMA(SmaFast), SMA(SmaSlow), CrossBarsBack))
				{
					ExitLong();
					
                    exitPrice = Open[-1]; // Take open price next day - [0] Current Bar Value, [1] Previous Bar Values, [-1] Next Bar Value
                    profitLoss = (exitPrice - entryPrice) * positionSize;
                    accountSize = accountSize + profitLoss;

					isInvested = false;
				}
			}        
		}
	
#region Properties

        [Description("Fast SMA")]
        [GridCategory("SMA")]
        public int SmaFast
        {
            get { return smaFast; }
            set { smaFast = Math.Max(1, value); } // Gibt die groessere von zwei Zahlen zurueck
        }

        [Description("Slow SMA")]
        [GridCategory("SMA")]
        public int SmaSlow
        {
            get { return smaSlow; }
            set { smaSlow = Math.Max(1, value); } // Gibt die groessere von zwei Zahlen zurueck
        }
		
        [Description("SMA Cross Above or Cross Below within this number of previous bars")]
        [GridCategory("SMA")]
        public int CrossBarsBack
        {
            get { return crossBarsBack; }
            set { crossBarsBack = Math.Max(1, value); } // Gibt die groessere von zwei Zahlen zurueck
        }

        #endregion
    }
}
