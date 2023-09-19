/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
INSERT INTO Symbol (Ticker, Region, Sector)
SELECT 'A', 'US', 'Health Care'
WHERE NOT EXISTS (SELECT 1 FROM Symbol WHERE Ticker = 'A');
INSERT INTO Symbol (Ticker, Region, Sector)
SELECT 'AA','US','Materials'
WHERE NOT EXISTS (SELECT 1 FROM Symbol WHERE Ticker = 'AA');

INSERT INTO Price (SymbolID, [Date], PriceOpen, PriceHigh, PriceLow, PriceClose, PriceAdj, Volume)
SELECT '1', '1/3/2000', '78.75', '78.937494', '67.375', '72.46991788', '46.991788', '4674400'
WHERE NOT EXISTS (SELECT 1 FROM Price WHERE SymbolID = 1 AND Date='1/3/2000');
INSERT INTO Price (SymbolID, [Date], PriceOpen, PriceHigh, PriceLow, PriceClose, PriceAdj, Volume)
SELECT '1', '1/4/2000', '68.125', '68.875', '64.75', '66.6', '43.40213', '4765100'
WHERE NOT EXISTS (SELECT 1 FROM Price WHERE SymbolID = 1 AND Date='1/4/2000');
INSERT INTO Price (SymbolID, [Date], PriceOpen, PriceHigh, PriceLow, PriceClose, PriceAdj, Volume)
SELECT '2', '1/3/2000', '83', '83.5625', '80.375', '80.9375', '30.672', '3103200'
WHERE NOT EXISTS (SELECT 1 FROM Price WHERE SymbolID = 2 AND Date='1/3/2000');
INSERT INTO Price (SymbolID, [Date], PriceOpen, PriceHigh, PriceLow, PriceClose, PriceAdj, Volume)
SELECT '2', '1/4/2000', '80.9375', '81.8125', '80.3125', '81.3125', '30.814179', '4469600'
WHERE NOT EXISTS (SELECT 1 FROM Price WHERE SymbolID = 2 AND Date='1/4/2000');
INSERT INTO Price (SymbolID, [Date], PriceOpen, PriceHigh, PriceLow, PriceClose, PriceAdj, Volume)
SELECT '2', '1/5/2000', '81.3125', '86.5', '81', '86', '32.590553', '6243200'
WHERE NOT EXISTS (SELECT 1 FROM Price WHERE SymbolID = 2 AND Date='1/5/2000');