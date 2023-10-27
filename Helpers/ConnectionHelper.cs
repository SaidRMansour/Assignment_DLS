using EasyNetQ;

namespace Helpers;

public static class ConnectionHelper
{
    public static IBus GetRMQConnection()
    {
        string connectionStr = "host=goose-01.rmq2.cloudamqp.com;virtualHost=jlnclkpv;username=jlnclkpv;password=rFayf87SEjxO2ZbvvDR5S1pZLVpdf145";

        return RabbitHutch.CreateBus(connectionStr);
    }
}