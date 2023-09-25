using System;

namespace QuantBook.Models.Strategy
{
    public struct DrawDownResult
    {
        public DateTime date;
        public double pnl;
        public double drawdown;
        public double drawup;
        public double pnlHold;
        public double drawdownHold;
        public double drawupHold;

        public DrawDownResult(DateTime date, double pnl, double drawdown, double drawup, double pnlHold, double drawdownHold, double drawupHold)
        {
            this.date = date;
            this.pnl = pnl;
            this.drawdown = drawdown;            
            this.drawup = drawup;            
            this.pnlHold = pnlHold;
            this.drawdownHold = drawdownHold;
            this.drawupHold = drawupHold;         
        }

        public override bool Equals(object obj)
        {
            return obj is DrawDownResult other &&
                   date == other.date &&
                   pnl == other.pnl &&
                   drawdown == other.drawdown &&
                   drawup == other.drawup &&
                   pnlHold == other.pnlHold &&
                   drawdownHold == other.drawdownHold &&
                   drawupHold == other.drawupHold;
        }

        public override int GetHashCode()
        {
            int hashCode = 1211010681;
            hashCode = hashCode * -1521134295 + date.GetHashCode();
            hashCode = hashCode * -1521134295 + pnl.GetHashCode();
            hashCode = hashCode * -1521134295 + drawdown.GetHashCode();
            hashCode = hashCode * -1521134295 + drawup.GetHashCode();
            hashCode = hashCode * -1521134295 + pnlHold.GetHashCode();
            hashCode = hashCode * -1521134295 + drawdownHold.GetHashCode();
            hashCode = hashCode * -1521134295 + drawupHold.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out DateTime date, out double pnl, out double drawdown, out double drawup, out double pnlHold, out double drawdownHold, out double drawupHold)
        {
            date = this.date;
            pnl = this.pnl;
            drawdown = this.drawdown;
            drawup = this.drawup;
            pnlHold = this.pnlHold;
            drawdownHold = this.drawdownHold;
            drawupHold = this.drawupHold;
        }

        //public static implicit operator (DateTime date, double pnl, double drawdown, double drawup, double maxDrawup, double pnlHold, double drawdownHold, double drawupHold)(DrawDownResult value)
        //{
        //    return (value.date, value.pnl, value.drawdown, value.drawup, value.pnlHold, value.drawdownHold, value.drawupHold);
        //}

        public static implicit operator DrawDownResult((DateTime date, double pnl, double drawdown, double drawup, double pnlHold, double drawdownHold, double drawupHold) value)
        {
            return new DrawDownResult(value.date, value.pnl, value.drawdown, value.drawup, value.pnlHold, value.drawdownHold, value.drawupHold);
        }
    }
}
