QuantBook

Skipped these sections below:
* Linear Anlaysis and Regressions R-Squared pg 207
* Technical Indicators using MSCharts pg 249
* Machine Learning pg 285
* Data Visualization Chapter pg143
	* Implemented MSChart and ignored the rest

* Pricing European Options using QuantLib pg 417
	* Implement other engines Binomiall Jarrow Rudd, Binomial Additive Quiprobabilities, Binomial Trigeorgis,
		Binomial Tian, Binomial Leisen Remier, Binomial Joshi, FiniteDifference, Integral, PseudoMonto Carlo, Quasi MonteCarlo
* Pricing Fixed Income 	
	* CDS Pricing Engine (big and interesting topic, it requires using IsdaHelper to fetch ISDA CDS standardized rates from the web which I think has been taken down)		
		* Hazard Rate valuation (need to think of alternative way to get hazard rates based on spreads, or use the quantlib features)
    * Callable Bonds
	* Convertible Bonds			
* Pricing Exotic Options using QuantLib (I already wrote the coded version, this about using the lib) pg 431
	* Barrier Options
	* Bermudan Options