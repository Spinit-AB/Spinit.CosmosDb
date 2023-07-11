using System;
using Newtonsoft.Json;
using Xunit;

namespace Spinit.CosmosDb.Tests.Unit.Internals
{
    public class DateSerializingTests
    {
        public class WhenUsingDateTime
        {
            [Fact]
            public void UtcShouldBeSerializedAsIso()
            {
                var utcNow = DateTime.UtcNow;
                var jsonValue = JsonConvert.SerializeObject(utcNow, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var utcFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'";
                var expected = utcNow.ToString(utcFormat);
                Assert.Equal(expected, jsonValue);
            }

            [Fact]
            public void LocalShouldBeSerializedAsIsoWithTimeZone()
            {
                var now = DateTime.Now;
                var jsonValue = JsonConvert.SerializeObject(now, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFzzz";
                var expected = now.ToString(format);
                Assert.Equal(expected, jsonValue);
            }

            [Fact]
            public void UnspecifiedShouldBeSerializedAsIso()
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                var jsonValue = JsonConvert.SerializeObject(now, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF";
                var expected = now.ToString(format);
                Assert.Equal(expected, jsonValue);
            }

            [Theory]
            [InlineData("2019-05-08T12:00Z")]
            [InlineData("2019-05-08T12:00")]
            public void RoundtripShouldResultInSameValue(string dateTimeString)
            {
                var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                var dateTime = DateTime.Parse(dateTimeString);
                var jsonValue = JsonConvert.SerializeObject(dateTime, serializerSettings);
                var value = JsonConvert.DeserializeObject<DateTime>(jsonValue, serializerSettings);
                Assert.Equal(dateTime, value);
            }

            [Theory]
            [InlineData("2019-05-08T12:00Z")]
            [InlineData("2019-05-08T12:00")]
            public void RoundtripShouldResultInSameKind(string dateTimeString)
            {
                var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                var dateTime = DateTime.Parse(dateTimeString);
                var jsonValue = JsonConvert.SerializeObject(dateTime, serializerSettings);
                var value = JsonConvert.DeserializeObject<DateTime>(jsonValue, serializerSettings);
                Assert.Equal(dateTime.Kind, value.Kind);
            }

            public class WithinObject
            {
                [Fact]
                public void UtcShouldBeSerializedAsIso()
                {
                    var utcNow = DateTime.UtcNow;
                    var entity = new TestEntity
                    {
                        DateTime = utcNow
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, CosmosDatabase.CreateJsonSerializerSettings());
                    var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'Z'";
                    var expected = $"{{\"DateTime\":\"{utcNow.ToString(format)}\"}}";
                    Assert.Equal(expected, jsonValue);
                }

                [Fact]
                public void LocalShouldBeSerializedAsIsoWithTimeZone()
                {
                    var now = DateTime.Now;
                    var entity = new TestEntity
                    {
                        DateTime = now
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, CosmosDatabase.CreateJsonSerializerSettings());
                    var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFzzz";
                    var expected = $"{{\"DateTime\":\"{now.ToString(format)}\"}}";
                    Assert.Equal(expected, jsonValue);
                }

                [Fact]
                public void UnspecifiedShouldBeSerializedAsIso()
                {
                    var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    var entity = new TestEntity
                    {
                        DateTime = now
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, CosmosDatabase.CreateJsonSerializerSettings());
                    var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF";
                    var expected = $"{{\"DateTime\":\"{now.ToString(format)}\"}}";
                    Assert.Equal(expected, jsonValue);
                }

                [Theory]
                [InlineData("2019-05-08T12:00Z")]
                [InlineData("2019-05-08T12:00")]
                public void ObjectRoundtripShouldResultInSameValue(string dateTimeString)
                {
                    var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                    var dateTime = DateTime.Parse(dateTimeString);
                    var entity = new TestEntity
                    {
                        DateTime = dateTime
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, serializerSettings);
                    var value = JsonConvert.DeserializeObject<TestEntity>(jsonValue, serializerSettings);
                    Assert.Equal(dateTime, value?.DateTime);
                }

                [Theory]
                [InlineData("2019-05-08T12:00Z")]
                [InlineData("2019-05-08T12:00")]
                public void ObjectRoundtripShouldResultInSameKind(string dateTimeString)
                {
                    var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                    var dateTime = DateTime.Parse(dateTimeString);
                    var entity = new TestEntity
                    {
                        DateTime = dateTime
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, serializerSettings);
                    var value = JsonConvert.DeserializeObject<TestEntity>(jsonValue, serializerSettings);
                    Assert.Equal(dateTime.Kind, value?.DateTime.Kind);
                }

                public class TestEntity
                {
                    public DateTime DateTime { get; set; }
                }
            }
        }

        public class WhenUsingDateTimeOffset
        {
            [Fact]
            public void UtcShouldBeSerializedWithOffset()
            {
                var utcNow = DateTimeOffset.UtcNow;
                var jsonValue = JsonConvert.SerializeObject(utcNow, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'+00:00'";
                var expected = utcNow.ToString(format);
                Assert.Equal(expected, jsonValue);
            }

            [Fact]
            public void LocalShouldBeSerializedWithOffset()
            {
                var now = DateTimeOffset.Now;
                var jsonValue = JsonConvert.SerializeObject(now, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'+0" + now.Offset.Hours + ":00'";
                var expected = now.ToString(format);
                Assert.Equal(expected, jsonValue);
            }

            [Fact]
            public void PacificShouldBeSerializedWith8HoursOffset()
            {
                var offset = TimeSpan.FromHours(8);
                var dateTimeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                var now = new DateTimeOffset(dateTimeNow, offset);
                var jsonValue = JsonConvert.SerializeObject(now, CosmosDatabase.CreateJsonSerializerSettings()).Trim('\"');
                var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF'+08:00'";
                var expected = now.ToString(format);
                Assert.Equal(expected, jsonValue);
            }

            [Fact]
            public void PacificShouldBeDeserializedWith8HoursOffset()
            {
                var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                var jsonValue = "\"2019-05-08T08:00:00.0000000+08:00\"";
                var result = JsonConvert.DeserializeObject<DateTimeOffset>(jsonValue, serializerSettings);
                Assert.Equal(8, result.Offset.Hours);
            }

            [Theory]
            [InlineData("2019-05-08T12:00+00:00")]
            [InlineData("2019-05-08T12:00+02:00")]
            [InlineData("2019-05-08T12:00+08:00")]
            [InlineData("2019-05-08T12:00-06:00")]
            public void RoundtripShouldResultInSameValue(string dateTimeString)
            {
                var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                var dateTimeOffset = DateTimeOffset.Parse(dateTimeString);
                var jsonValue = JsonConvert.SerializeObject(dateTimeOffset, serializerSettings);
                var value = JsonConvert.DeserializeObject<DateTimeOffset>(jsonValue, serializerSettings);
                Assert.Equal(dateTimeOffset, value);
            }

            [Theory]
            [InlineData("2019-05-08T12:00+00:00")]
            [InlineData("2019-05-08T12:00+02:00")]
            [InlineData("2019-05-08T12:00+08:00")]
            [InlineData("2019-05-08T12:00-06:00")]
            public void RoundtripShouldResultInSameOffset(string dateTimeString)
            {
                var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                var dateTimeOffset = DateTimeOffset.Parse(dateTimeString);
                var jsonValue = JsonConvert.SerializeObject(dateTimeOffset, serializerSettings);
                var value = JsonConvert.DeserializeObject<DateTimeOffset>(jsonValue, serializerSettings);
                Assert.Equal(dateTimeOffset.Offset, value.Offset);
            }

            public class WithinObject
            {
                [Theory]
                [InlineData("2019-05-08T12:00+00:00")]
                [InlineData("2019-05-08T12:00+02:00")]
                [InlineData("2019-05-08T12:00+08:00")]
                [InlineData("2019-05-08T12:00-06:00")]
                public void ObjectRoundtripShouldResultInSameValue(string dateTimeString)
                {
                    var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                    var dateTimeOffset = DateTimeOffset.Parse(dateTimeString);
                    var entity = new TestEntity
                    {
                        DateTimeOffset = dateTimeOffset
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, serializerSettings);
                    var value = JsonConvert.DeserializeObject<TestEntity>(jsonValue, serializerSettings);
                    Assert.Equal(dateTimeOffset, value?.DateTimeOffset);
                }

                [Theory]
                [InlineData("2019-05-08T12:00+00:00")]
                [InlineData("2019-05-08T12:00+02:00")]
                [InlineData("2019-05-08T12:00+08:00")]
                [InlineData("2019-05-08T12:00-06:00")]
                public void ObjectRoundtripShouldResultInSameOffset(string dateTimeString)
                {
                    var serializerSettings = CosmosDatabase.CreateJsonSerializerSettings();
                    var dateTimeOffset = DateTimeOffset.Parse(dateTimeString);
                    var entity = new TestEntity
                    {
                        DateTimeOffset = dateTimeOffset
                    };
                    var jsonValue = JsonConvert.SerializeObject(entity, serializerSettings);
                    var value = JsonConvert.DeserializeObject<TestEntity>(jsonValue, serializerSettings);
                    Assert.Equal(dateTimeOffset.Offset, value?.DateTimeOffset.Offset);
                }

                public class TestEntity
                {
                    public DateTimeOffset DateTimeOffset { get; set; }
                }
            }
        }
    }
}
