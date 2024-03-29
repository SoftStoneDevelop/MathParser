﻿using MathEngine.Helpers;
using NUnit.Framework;
using System;

namespace MathEngineTests
{
    [TestFixture]
    internal class ParserHelperFixture
    {
        [Test]
        public void IsNumber()
        {
            Assert.That(ParserHelper.IsNumber("0".AsSpan(), false), Is.EqualTo(1));
            Assert.That(ParserHelper.IsNumber("0+1".AsSpan(), false), Is.EqualTo(1));
            Assert.That(ParserHelper.IsNumber("0 + 1".AsSpan(), false), Is.EqualTo(1));
            Assert.That(ParserHelper.IsNumber("0.132544".AsSpan(), false), Is.EqualTo(8));
            Assert.That(ParserHelper.IsNumber("456213".AsSpan(), false), Is.EqualTo(6));
            
            Assert.That(ParserHelper.IsNumber("456.5 2135".AsSpan(), false), Is.EqualTo(5));

            Assert.That(ParserHelper.IsNumber(" 456213".AsSpan(), false), Is.EqualTo(-1));
            Assert.That(ParserHelper.IsNumber("s".AsSpan(), false), Is.EqualTo(-1));
            Assert.That(ParserHelper.IsNumber("!".AsSpan(), false), Is.EqualTo(-1));

            Assert.That(
                () => ParserHelper.IsNumber("001254.5".AsSpan(), false),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Incorrect number")
                );

            Assert.That(
                () => ParserHelper.IsNumber("12.54.5".AsSpan(), false),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Double separator in number '12.54.'")
                );

            Assert.That(
                () => ParserHelper.IsNumber("12.".AsSpan(), false),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Unexpected end of number '12.'")
                );

            Assert.That(
                () => ParserHelper.IsNumber(Span<char>.Empty, false),
                Throws.Exception.TypeOf(typeof(ArgumentException)).And
                .Message.EqualTo("Empty span")
                );
        }
    }
}