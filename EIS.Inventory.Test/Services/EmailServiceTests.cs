using System;
using EIS.Inventory.Core.Services;
using AutoMapper;
using EIS.Inventory.Core.Mappings;

namespace EIS.Inventory.Test
{
    //[TestClass]
    public class UnitTest_EmailService
    {
        private EmailNotificationService _emailService;

        //[TestInitialize]
        public void InitializeVariables()
        {
            _emailService = new EmailNotificationService();

            Mapper.Initialize(x =>
            {
                x.AddProfile<ViewModelToDomainMappingProfile>();
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
        }


        //[TestMethod]
        public void Should_Send_Email_With_Defaults()
        {
            // Arrange
            var subject = "test send notif";
            var mailTo = "rosal.alvincent@gmail.com";
            var body = "Hello World!";


            // Act
            var result = _emailService.SendEmail(subject, body, mailTo);


            // Assert
            //Assert.IsTrue(result, "Email sending failed.");
        }


        //[TestMethod]
        public void Should_Send_Email_Without_Defaults()
        {
            // Arrange
            var subject = "test send notif";
            var mailTo = "rosal.alvincent@gmail.com";
            var body = "Hello World!";


            // Act
            var result = _emailService.SendEmail(subject, body, mailTo);


            // Assert
            //Assert.IsTrue(result, "Email sending failed.");
        }
    }
}
