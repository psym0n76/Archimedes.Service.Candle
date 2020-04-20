using System;
using NUnit.Framework;

namespace Archimedes.Service.Candle.Tests
{
    [TestFixture]
    public class DateTimeExtensionTests
    {
        [TestCase("2020-04-17T17:01:00","2020-04-17T16:55:00",5)]
        [TestCase("2020-04-17T17:01:01","2020-04-17T16:55:00",5)]
        [TestCase("2020-04-17T17:02:00","2020-04-17T16:55:00",5)]
        [TestCase("2020-04-17T17:04:00","2020-04-17T16:55:00",5)]
        [TestCase("2020-04-17T17:05:01","2020-04-17T17:00:00",5)]
        [TestCase("2020-04-17T17:05:01","2020-04-17T16:45:00",15)]
        [TestCase("2020-04-17T17:09:01","2020-04-17T16:45:00",15)]
        [TestCase("2020-04-17T17:15:01","2020-04-17T17:00:00",15)]
        public void Should_return_rounded_down_time(DateTime inputDate, DateTime expectedDate, int interval)
        {
            //Assert.That(inputDate, Is.EqualTo(new DateTime(2020,04,17,17,00,00)));

            var subject = inputDate;

            var result = subject.RoundDownTime(interval);

            Assert.That(result,Is.EqualTo(expectedDate));
        }
    }
}