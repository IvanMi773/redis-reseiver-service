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
        private Root root;
        private Mock<IBlockedQueueService> blockedQueueServiceMock;
        private Mock<IServiceBusSenderService> serviceBusSenderServiceMock;

        public EventConsumerServiceTest()
        {
            root = new() {Id = "test", Timestamp = "test", Type = "test", UserId = "test", DeviceId = "test"};
            blockedQueueServiceMock = new Mock<IBlockedQueueService>();
            serviceBusSenderServiceMock = new Mock<IServiceBusSenderService>();
        }
        
        [Fact]
        public void ConsumeMessagesFromQueue_CollectionIsCompleted_EndTask()
        {
            // Arrange
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
        public void ConsumeMessagesFromQueue_EmptyCollection_NothingWasSent()
        {
            // Arrange
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(100);

            // Assert
            serviceBusSenderServiceMock.Verify(service => service.SendMessage(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public void ConsumeMessagesFromQueue_FiveMessagesInQueue_SendInOneBatch()
        {
            // Arrange
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);
            blockedQueueServiceMock.SetupSequence(service => service.Take(It.IsAny<int>()))
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(() =>
                {
                    Thread.Sleep(2000);
                    return null;
                });

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(2500);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Exactly(1)
            );
        }

        [Fact]
        public void ConsumeMessagesFromQueue_SixMessagesInQueue_SendInTwoBatches()
        {
            // Arrange
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);
            blockedQueueServiceMock.SetupSequence(service => service.Take(It.IsAny<int>()))
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(root)
                .Returns(() =>
                {
                    Thread.Sleep(2000);
                    return null;
                });

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(2500);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Exactly(2)
            );
        }
        
        [Fact]
        public void ConsumeMessagesFromQueue_OneElementInQueue_SendMessages()
        {
            // Arrange
            blockedQueueServiceMock.Setup(service => service.IsCompleted()).Returns(false);
            blockedQueueServiceMock.SetupSequence(service => service.Take(It.IsAny<int>()))
                .Returns(root)
                .Returns(() =>
                {
                    Thread.Sleep(2000);
                    return null;
                });

            var eventConsumerService =
                new EventConsumerService(blockedQueueServiceMock.Object, serviceBusSenderServiceMock.Object);

            // Act
            eventConsumerService.ConsumeMessagesFromQueue();
            Thread.Sleep(2500);

            // Assert
            serviceBusSenderServiceMock.Verify(
                service => service.SendMessage(It.IsAny<List<string>>()),
                Times.Exactly(1)
            );
        }
    }
}