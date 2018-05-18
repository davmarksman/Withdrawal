using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using System;

namespace Moneybox.App.Test
{
    [TestFixture]
    public class TransferMoneyTests
    {
        [Test]
        public void WhenSenderInsufficentFunds_Throw()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fakeNotificationService = new Mock<INotificationService>();

            fakeAccountRepo.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(new Account { Balance = 10 });

            var transferMoney = new TransferMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var toGuid = new Guid("7c8cb0f1-a101-4563-8a22-0f4cfdf56e80");
            
            Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(fromGuid, toGuid, 100));
        }

        [Test]
        public void WhenSenderLowFunds_Notify()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var toGuid = new Guid("7c8cb0f1-a101-4563-8a22-0f4cfdf56e80");
            var senderEmail = "sender@co.uk";
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(new Account { Balance = 200, Withdrawn=0,  User = new User { Email = senderEmail } });
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == toGuid))).Returns(new Account { Balance = 200, PaidIn = 0, });

            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new TransferMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            transferMoney.Execute(fromGuid, toGuid, 100);

            fakeNotificationService.Verify(x => x.NotifyFundsLow(senderEmail));
        }
    
        [Test]
        public void WhenRecieverPaymentLimit_Throw()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var toGuid = new Guid("7c8cb0f1-a101-4563-8a22-0f4cfdf56e80");
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == toGuid))).Returns(new Account { Balance = 5000, PaidIn = 3100 });
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(new Account { Balance = 2000, Withdrawn = 0});

            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new TransferMoney(fakeAccountRepo.Object, fakeNotificationService.Object);

            Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(fromGuid, toGuid, 1000));
        }
    
        [Test]
        public void WhenNearRecieverPaymentLimit_NotifyReciever()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var toGuid = new Guid("7c8cb0f1-a101-4563-8a22-0f4cfdf56e80");
            var recieverEmail = "reciever@co.uk";
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == toGuid))).Returns(new Account { Balance = 1000, PaidIn = 3450,  User = new User { Email = recieverEmail } });
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(new Account { Balance = 2000, Withdrawn = 0, });

            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new TransferMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            transferMoney.Execute(fromGuid, toGuid, 100);

            fakeNotificationService.Verify(x => x.NotifyApproachingPayInLimit(recieverEmail));
        }

        [Test]
        public void SuccesfullPaymentTest()
        {
            var fakeAccountRepo = new Mock<IAccountRepository>();
            var fromGuid = new Guid("3605a4f9-96a4-439e-9155-064a94736e9c");
            var toGuid = new Guid("7c8cb0f1-a101-4563-8a22-0f4cfdf56e80");
            var reciever = CreateAccount();
            var sender = CreateAccount();

            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == fromGuid))).Returns(sender);
            fakeAccountRepo.Setup(x => x.GetAccountById(It.Is<Guid>(z => z == toGuid))).Returns(reciever);

            var fakeNotificationService = new Mock<INotificationService>();

            var transferMoney = new TransferMoney(fakeAccountRepo.Object, fakeNotificationService.Object);
            transferMoney.Execute(fromGuid, toGuid, 100);

            Assert.That(sender.Withdrawn, Is.EqualTo(-100));
            Assert.That(sender.Balance, Is.EqualTo(900));
            Assert.That(reciever.Balance, Is.EqualTo(1100));
            Assert.That(reciever.PaidIn, Is.EqualTo(100));
        }

        private static Account CreateAccount()
        {
            return new Account { Balance = 1000, PaidIn = 0, Withdrawn = 0, User = new User { Email = "test" } };
        }
    }
}
