using MathEngine.Helpers;
using NUnit.Framework;
using System;

namespace MathEngineTests
{
    [TestFixture]
    public class ParserHelperFixture
    {
        [Test]
        public void IsNumber()
        {
            Assert.That(ParserHelper.IsNumber("0".AsSpan()), Is.EqualTo(1));
            Assert.That(ParserHelper.IsNumber("0.132544".AsSpan()), Is.EqualTo(8));
            Assert.That(ParserHelper.IsNumber("456213".AsSpan()), Is.EqualTo(6));
            
            Assert.That(ParserHelper.IsNumber("456.5 2135".AsSpan()), Is.EqualTo(5));

            Assert.That(ParserHelper.IsNumber(" 456213".AsSpan()), Is.EqualTo(-1));
            Assert.That(ParserHelper.IsNumber("s".AsSpan()), Is.EqualTo(-1));
            Assert.That(ParserHelper.IsNumber("!".AsSpan()), Is.EqualTo(-1));

            Assert.That(
                () => ParserHelper.IsNumber("001254.5".AsSpan()),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Incorrect number")
                );

            Assert.That(
                () => ParserHelper.IsNumber("12.54.5".AsSpan()),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Double separator in number '12.54.'")
                );

            Assert.That(
                () => ParserHelper.IsNumber("12.".AsSpan()),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Unexpected end of number '12.'")
                );

            Assert.That(
                () => ParserHelper.IsNumber(Span<char>.Empty),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Empty span")
                );
        }
    }
}