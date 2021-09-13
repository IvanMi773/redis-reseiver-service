using System.Collections.Generic;
using System.Threading;
using Moq;
using ReceiverService.Entities;
using ReceiverService.Services.BlockedQueue;
using ReceiverService.Services.Events;
using ReceiverService.Services.ServiceBus;
using Xunit;

namespace ReceiverServiceTests.Services.Events
{
    public class EventConsumerServiceTest
    {
        private Root root = new() {Id = "test", Timestamp = "test", Type = "test", UserId = "test", DeviceId = "test"};

        [Fact]
        public void Should_Return_When_Blocked_Collection_Is_Completed()
        {
            // Arrange
            var blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            var serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(true);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            var task = eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(100);

            // Assert
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Shouldnt_Send_Messages_When_Blocking_Collection_Is_Empty()
        {
            // Arrange
            var blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            var serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();

            // Assert
            serviceBusSenderServiceMock.Verify(service => service.SendMessage(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public void Should_Add_Five_Messages_And_Send_In_One_Batch()
        {
            // Arrange
            var blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            var serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);
            blockedQueueServiceMock.SetupSequence(service => service.Take(It.IsAny<int>()))
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(500);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Exactly(1)
            );
        }

        [Fact]
        public void Should_Add_Six_Messages_And_Send_In_Two_Batches()
        {
            // Arrange
            var blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            var serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);
            blockedQueueServiceMock.SetupSequence(service => service.Take(It.IsAny<int>()))
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(500);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Exactly(2)
            );
        }
        
        [Fact]
        public void Shouldnt_Throw_When_Root_Is_Null()
        {
            // Arrange
            var blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            var serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(100);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Never
            );
        }
    }
}