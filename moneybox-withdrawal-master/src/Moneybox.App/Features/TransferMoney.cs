using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            if (!from.CanWithdraw(amount))
                throw new InvalidOperationException("Insufficient funds to make transfer");

            // Need to notify user low balance even if execute fails on payin step
            if (from.WillBeLowBalanceAfterWithdrawal(amount))
                this.notificationService.NotifyFundsLow(from.User.Email);

            to.PayIn(amount); // will throw if cannot pay in
            from.Withdraw(amount);

            if (to.NearPayInLimit)
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);

            this.accountRepository.Update(from);
            this.accountRepository.Update(to);
        }
    }
}
