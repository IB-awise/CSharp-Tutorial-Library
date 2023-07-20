﻿using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class tickByTickTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void tickByTickTestMain()
        {
            var testImpl = new tickByTickTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            while (testImpl.NextOrderId <= 0) { }

            Contract contract = new Contract();
            contract.Symbol = "AAL";
            contract.SecType = "STK";
            contract.Exchange = "SMART";
            contract.Currency = "USD";

            Console.WriteLine("Requesting market data for " + contract.Symbol);
            clientSocket.reqTickByTickData(testImpl.NextOrderId, contract, "Trades", 100, false);

            // We can stream data forever, but we'll kill the connection after 1000 milliseconds.
            Thread.Sleep(10000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        public override void tickByTickAllLast(int reqId, int tickType, long time, double price, decimal size, TickAttribLast tickAttribLast, string exchange, string specialConditions)
        {
            Console.WriteLine("tickByTickAllLast", reqId, tickType, time, price, size, tickAttribLast, exchange, specialConditions);
        }

        public override void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, decimal bidSize, decimal askSize, TickAttribBidAsk tickAttribBidAsk)
        {
            Console.WriteLine("tickByTickBidAsk", reqId, time, bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk);
        }

        public override void tickByTickMidPoint(int reqId, long time, double midPoint)
        {
            Console.WriteLine("tickByTickMidPoint", reqId, time, midPoint);
        }



        //! [socket_init]
        public tickByTickTest()
        {
            Signal = new EReaderMonitorSignal();
            clientSocket = new EClientSocket(this, Signal);
        }
        //! [socket_init]

        public EClientSocket ClientSocket
        {
            get { return clientSocket; }
            set { clientSocket = value; }
        }

        public int NextOrderId
        {
            get { return nextOrderId; }
            set { nextOrderId = value; }
        }

        public override void nextValidId(int orderId)
        {
            Console.WriteLine("Next Valid Id: " + orderId);
            NextOrderId = orderId;
        }

        public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson)
        {
            if (!Util.StringIsEmpty(advancedOrderRejectJson))
            {
                Console.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + ", AdvancedOrderRejectJson: " + advancedOrderRejectJson + "\n");
            }
            else
            {
                Console.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            }
        }
    }
}
