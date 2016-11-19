using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public Mock<IProducerConsumerCollection<int>> Collection { get; set; }

        [TestInitialize]
        public void Init()
        {
            Collection = new Mock<IProducerConsumerCollection<int>>();
        }

        [TestMethod]
        public void Should_Check_The_Initial_Count()
        {
            // Arrange
            Collection.Setup(x => x.Count).Returns(3).Verifiable();

            // Act
            new BlockingCollection<int>(Collection.Object, 3);

            // Assert
            Collection.VerifyAll();
        }

        [TestMethod]
        public void Should_Keep_Its_Own_Count()
        {
            // Arrange
            var collection = new ConcurrentBag<int> { 5 };
            var blocking = new BlockingCollection<int>(collection, 3);
            int output;

            // Act
            collection.TryTake(out output);
            Action a = () => blocking.Take();

            // Assert
            collection.Count.Should().Be(0);
            blocking.Count.Should().Be(1);
            a.ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
        public void Should_Check_For_Consistency()
        {
            // Arrange
            Collection.Setup(x => x.Count).Returns(5).Verifiable();

            // Act
            Action a = () => new BlockingCollection<int>(Collection.Object, 3);

            // Assert
            a.ShouldThrow<System.ArgumentException>();
        }

        [TestMethod]
        public void Should_Bound()
        {
            // Arrange
            Collection.Setup(x => x.Count).Returns(0).Verifiable();
            Collection.Setup(x => x.TryAdd(It.IsAny<int>()))
                      .Returns(true).Verifiable();
            var blocking = new BlockingCollection<int>(Collection.Object, 3);

            // Act
            blocking.Add(1);
            blocking.Add(2);

            var beforeBounding = blocking.TryAdd(3, 0);
            var afterBounding = blocking.TryAdd(4, 0);

            // Assert
            beforeBounding.Should().Be(true);
            afterBounding.Should().Be(false);
        }

        [TestMethod]
        public void Should_Block()
        {
            // Arrange
            int output = 0;
            Collection.Setup(x => x.Count).Returns(3).Verifiable();
            Collection.Setup(x => x.TryTake(out output)).Returns(true);
            var blocking = new BlockingCollection<int>(Collection.Object);

            // Act
            blocking.Take();
            blocking.Take();
            var beforeBlocking = blocking.TryTake(out output, 0);
            var afterBlocking = blocking.TryTake(out output, 0);

            // Assert
            beforeBlocking.Should().Be(true);
            afterBlocking.Should().Be(false);
        }

        [TestMethod]
        public void Should_Timeout_Only_On_Blocks_Or_Bounds()
        {
            // Arrange
            Collection.Setup(x => x.Count).Returns(0).Verifiable();
            Collection.Setup(x => x.TryAdd(It.IsAny<int>()))
                      .Callback(() => Task.Delay(1000).Wait())
                      .Returns(true);
            var blocking = new BlockingCollection<int>(Collection.Object, 3);

            // Act
            var isAdded = blocking.TryAdd(1, 0);

            // Assert
            isAdded.Should().Be(true);
        }

        [TestMethod]
        public void Should_Throw_An_Exception_When_Cancelling()
        {
            // Arrange
            int output;
            Collection.Setup(x => x.Count).Returns(0).Verifiable();
            var blocking = new BlockingCollection<int>(Collection.Object, 3);
            CancellationTokenSource cts = new CancellationTokenSource();

            // Act
            cts.Cancel();
            Action a = () => blocking.TryTake(out output,0, cts.Token);

            // Assert
            a.ShouldThrow<OperationCanceledException>();
            cts.Token.IsCancellationRequested.Should().Be(true);
        }

        [TestMethod]
        public void Should_Throw_An_Exception_When_Completed()
        {
            // Arrange
            int output;
            Collection.Setup(x => x.Count).Returns(0).Verifiable();
            Collection.Setup(x => x.TryTake(out output)).Returns(true);
            var blocking = new BlockingCollection<int>(Collection.Object, 3);

            // Act
            blocking.CompleteAdding();
            Action a = () => blocking.Take();
            var result = blocking.TryTake(out output);

            // Assert
            a.ShouldThrow<InvalidOperationException>();
            result.Should().Be(false);
        }

        [TestMethod]
        public void Should_Provide_A_Consuming_Enumerable()
        {
            // Arrange
            int output = 0;
            Collection.Setup(x => x.Count).Returns(1).Verifiable();
            Collection.Setup(x => x.TryTake(out output)).Returns(true).Verifiable();
            var blocking = new BlockingCollection<int>(Collection.Object, 3);

            // Act
            foreach(var i in blocking.GetConsumingEnumerable())
            {
                blocking.CompleteAdding();
            }

            // Assert
            Collection.VerifyAll();
            blocking.Count.Should().Be(0);
        }

        [TestMethod]
        public void Should_Provide_An_Enumerable_From_Collection()
        {
            // Arrange
            Collection.Setup(x => x.Count).Returns(1).Verifiable();
            Collection.Setup(x => x.GetEnumerator()).Returns(GetAnEnumerator()).Verifiable();
            var blocking = new BlockingCollection<int>(Collection.Object, 3);
            int result = 0;

            // Act
            foreach (var i in blocking)
            {
                result = i;
            }

            // Assert
            Collection.VerifyAll();
            blocking.Count.Should().Be(1);
            result.Should().Be(5);
        }

        IEnumerator<int> GetAnEnumerator()
        {
            yield return 5;
        }
    }
}
