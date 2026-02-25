using SMTPRouter.Abstractions;
using SMTPRouter.Core.RoutingRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.UnitTesting;

[TestClass]
[DoNotParallelize]
public sealed class TestRoutingRules
{

    [TestMethod]
    public void TestOriginatedAtRoutingRule()
    {
        var r = new OriginatedAtRoutingRule()
        {
            IPAddress = "10.0.0.51"
        };

        var m = new TestSmtpMessage()
        {
            Sender = "test@domain.com",
            Contents = string.Empty,
            OriginIPAddress = "10.0.0.51"
        };

        Assert.IsTrue(r.Match(m));
    }
}

internal sealed class TestSmtpMessage : IMessage
{
    public string? OriginIPAddress { get; set; }
    public string? Sender { get; set; }
    public string? Contents { get; set; }
}