using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using System;

namespace Moneybox.App.Test
{
    [TestFixture]
    public class WithdrawMoneyTests
    {
        [Test]
        public void WhenWithdrawningNegativeAmount_Throw()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fakeNotificationService = new Mock<INotificationService>();

            fakeAccountRepo.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(new Account { Balance = 1000 });

            var transferMoney = new WithdrawMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");

            Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(fromGuid,- 100));
        }

        [Test]
        public void WhenSenderInsufficentFunds_Throw()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fakeNotificationService = new Mock<INotificationService>();

            fakeAccountRepo.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(new Account { Balance = 10 });

            var transferMoney = new WithdrawMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            
            Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(fromGuid, 100));
        }

        [Test]
        public void WhenSenderLowFunds_Notify()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var fromEmail = "reciever@co.uk";
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(new Account { Balance = 200, Withdrawn=0,  User = new User { Email = fromEmail } });
            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new WithdrawMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            transferMoney.Execute(fromGuid, 100);

            fakeNotificationService.Verify(x => x.NotifyFundsLow(fromEmail));
        }

        [Test]
        public void SuccesfullWithdrawalTest()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var fromAccount = CreateAccount();
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(fromAccount);
            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new WithdrawMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            transferMoney.Execute(fromGuid, 130);

            Assert.That(fromAccount.Withdrawn, Is.EqualTo(-130));
            Assert.That(fromAccount.Balance, Is.EqualTo(870));
        }

        private static Account CreateAccount()
        {
            return new Account { Balance = 1000, PaidIn = 0, Withdrawn = 0, User = new User { Email = "test" } };
        }
    }
}
