using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal LowBalanceThreshold = 500m;
        public const decimal BalanceLimitForWithdraw = 0m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        private void GuardPositive(decimal amount)
        {
            if (amount < 0)
                throw new InvalidOperationException("Amount should be positive");
        }

        public void Withdraw(decimal amount)
        {
            GuardPositive(amount);

            if (!CanWithdraw(amount))
                throw new InvalidOperationException("Insufficient funds to make transfer");

            this.Balance = this.Balance - amount;
            this.Withdrawn = this.Withdrawn - amount; // why this neg? expected positive
        }

        public bool CanWithdraw(decimal amount)
        {
            var newBalance = this.Balance - amount;
            return newBalance >= BalanceLimitForWithdraw;
        }

        public bool IsLowBalance()
        {
            return WillBeLowBalanceAfterWithdrawal(0);
        }

        public bool WillBeLowBalanceAfterWithdrawal(decimal amount)
        {
            var newBalance = this.Balance - amount;
            return newBalance < LowBalanceThreshold;
        }

        public void PayIn(decimal amount)
        {
            GuardPositive(amount);

            if (!CanPayIn(amount))
                throw new InvalidOperationException("Account pay in limit reached");

            this.Balance = this.Balance + amount;
            this.PaidIn = this.PaidIn + amount; // why this neg? expected positive
        }

        public bool CanPayIn(decimal amount)
        {
            GuardPositive(amount);

            var paidIn = this.PaidIn + amount;
            return paidIn <= PayInLimit;
        }

        public bool NearPayInLimit
        {
            get
            {
                return PayInLimit - PaidIn < 500m;
            }
        }
    }
}
