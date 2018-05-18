using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moneybox.App.Test
{
    [TestFixture]
    public class AccountTests
    {
        [Test]
        public void WhenWithdrawal_WillHaveNegativeBalance_ThrowException()
        {
            var account = new Account() { Balance = 1000 };

            Assert.Throws<InvalidOperationException>(() => account.Withdraw(2000));
        }

        [Test]
        public void WhenWithdrawal_WillHavePositiveBalance_CanWithdraw()
        {
            var account = new Account() { Balance = 1000 };

            account.Withdraw(100);

            Assert.That(account.Withdrawn, Is.EqualTo(-100));
            Assert.That(account.Balance, Is.EqualTo(900));
        }

        [Test]
        public void WhenWithdrawal_WillHaveZeroBalance_CanWithdraw()
        {
            var account = new Account() { Balance = 1000 };

            account.Withdraw(1000);

            Assert.That(account.Withdrawn, Is.EqualTo(-1000));
            Assert.That(account.Balance, Is.EqualTo(0));
        }

        [Test]
        public void WhenBalanceAlreadyBelow500_IsLowBalance()
        {
            var account = new Account() { Balance = 200 };

            Assert.True(account.WillBeLowBalanceAfterWithdrawal(100));
        }

        [Test]
        public void WhenBalanceWillBeBelow500_IsLowBalance()
        {
            var account = new Account() { Balance = 600 };

            Assert.True(account.WillBeLowBalanceAfterWithdrawal(200));
        }

        [Test]
        public void WhenBalanceAbove500_IsNotLowBalance()
        {
            var account = new Account() { Balance = 10000 };

            Assert.False(account.WillBeLowBalanceAfterWithdrawal(100));
        }

        [Test]
        public void WhenPayIn_WillBeAbovePayInLimit_ThrowException()
        {
            var account = new Account() { Balance = 1000, PaidIn = 500m };

            Assert.Throws<InvalidOperationException>(() => account.PayIn(Account.PayInLimit - 200m));
        }

        [Test]
        public void WhenPayIn_WillBeBelowPayInLimit_CanPayIn()
        {
            var account = new Account() { Balance = 1000, PaidIn = 200 };

            account.PayIn(100);

            Assert.That(account.PaidIn, Is.EqualTo(300));
            Assert.That(account.Balance, Is.EqualTo(1100));
        }

        [Test]
        public void WhenPayIn_WillReachPayInLimit_CanPayIn()
        {
            var account = new Account() { Balance = 1000m, PaidIn = 3000m };

            account.PayIn(Account.PayInLimit - 3000m);

            Assert.That(account.PaidIn, Is.EqualTo(Account.PayInLimit));
            Assert.That(account.Balance, Is.EqualTo(Account.PayInLimit - 2000m));
        }
    }
}
