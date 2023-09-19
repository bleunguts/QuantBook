CREATE TABLE [dbo].[Price]
(
	[PriceID] INT NOT NULL PRIMARY KEY IDENTITY,
	[SymbolID] INT NOT NULL,
	[Date] DATETIME NULL,
	[PriceOpen] FLOAT NULL,
	[PriceHigh] FLOAT NULL,
	[PriceLow] FLOAT NULL,
	[PriceClose] FLOAT NULL,
	[PriceAdj] FLOAT NULL,
	[Volume] FLOAT NULL,
	CONSTRAINT [FK_Price_Symbol] FOREIGN KEY ([SymbolID]) REFERENCES [Symbol]([SymbolID]),
	CONSTRAINT [UQ_Price] UNIQUE ([SymbolID], [Date])
)
