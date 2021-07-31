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
	* CDS Pricing Engine largely done
		* Use InterpolatedHazardRate curve by constructing a list of hazard rates by building CDS and getting the impliedHazardRate
		* last todo CDS Pricing on UI to use spread based hazard term structure instead of fixed hazard rate
    * Callable Bonds
	* Convertible Bonds			
* Pricing Exotic Options using QuantLib (I already wrote the coded version, this about using the lib) pg 431
	* Barrier Options
	* Bermudan Options