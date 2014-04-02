using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using AutomaticTest;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TradeAPITypes.Data;

namespace Applenium
{
    internal class OpenBookValidator
    {
        public class ValidationResponse
        {
            public bool Successful { get; set; }
            public string Information { get; set; }
        }

        private readonly DataAccessor _dataAccess;
        private readonly AppleniumLogger logger = new AppleniumLogger();

        public OpenBookValidator(bool isReal)
        {
            //TestFlowInfo tfi = new TestFlowInfo(isReal, "OpenBookValidator");
            //_dataAccess = new DataAccessor( isReal,ref tfi);
        }

        internal List<IWebElement> GetOpenBookPositionData(RemoteWebDriver driver, int tableRowweguiMapId)
        {
            try
            {

                var sl = new Selenium();
                //TestFlowInfo tfi = new TestFlowInfo(true, "OpenBookValidator");
                //var dataAccess = new DataAccessor(true, ref tfi);
                var guimapadapter = new GuiMapTableAdapter();
                string userNameElementValue = guimapadapter.GetTagTypeValue(Constants.ObOpenTradesUserName);
                string userName = driver.FindElement((By)sl.GetWebElement(1, userNameElementValue, string.Empty)).Text;
                string userTableRowElementValue = guimapadapter.GetTagTypeValue(tableRowweguiMapId);
                ReadOnlyCollection<IWebElement> rows =
                    driver.FindElements((By)sl.GetWebElement(1, userTableRowElementValue, string.Empty));
                List<IWebElement> pos = rows.ToList();


                //var openbookpositiondata = new List<OpenBookPositionData>();
                //foreach (IWebElement row in rows)
                //{
                //    StockStatus? stockStatus = StockStatus.Position;
                //    string parentUserNameElementValue =
                //        guimapadapter.GetTagTypeValue(Constants.ObOpenTardesParentNameGuiMapId);
                //    string parentUserName =
                //        row.FindElement((By)sl.GetWebElement(1, parentUserNameElementValue, string.Empty)).Text;
                //    string initRateStringElementValue =
                //        guimapadapter.GetTagTypeValue(Constants.ObOpenTardesInitRateStringGuimapid);
                //    string initRateString =
                //        row.FindElement((By)sl.GetWebElement(1, initRateStringElementValue, string.Empty)).Text;
                //    decimal initRate;

                //    if (initRateString != string.Empty)
                //    {
                //        String trimmedRate = sl.GetCleanRate(initRateString);
                //        initRate = Convert.ToDecimal(trimmedRate);
                //    }
                //    else
                //    {
                //        initRate = 0;
                //    }
                //    string instrumentDisplayNameElementValue =
                //        guimapadapter.GetTagTypeValue(Constants.ObOpenTradedInstrumentDisplayName);
                //    string instrumentDisplayName =
                //        row.FindElement((By)sl.GetWebElement(1, instrumentDisplayNameElementValue, string.Empty))
                //           .Text.Replace("/", "");
                //    string isBuyStringElementValue = guimapadapter.GetTagTypeValue(Constants.ObOpenTradesIsBuyString);
                //    string isBuyString =
                //        row.FindElement((By)sl.GetWebElement(1, isBuyStringElementValue, string.Empty)).Text;
                //    bool isBuy;
                //    if (String.Compare(isBuyString, "BUY", StringComparison.OrdinalIgnoreCase) == 0)
                //        isBuy = true;
                //    else
                //        isBuy = false;
                //    string isOverWeekendNameElementValue =
                //        guimapadapter.GetTagTypeValue(Constants.ObOpenTradesIsOverWeekendNonStocks);
                //    bool isOverWeekend = sl.CheckIfElementFind(row,
                //                                            (By)sl.GetWebElement(1, isOverWeekendNameElementValue, string.Empty),
                //                                            driver);
                //    if (isOverWeekend == false)
                //    {
                //        //non stocks
                //        isOverWeekendNameElementValue =
                //            guimapadapter.GetTagTypeValue(Constants.ObOpenTradesIsOverWeekendStocks);
                //        isOverWeekend = sl.CheckIfElementFind(row, (By)sl.GetWebElement(1, isOverWeekendNameElementValue, string.Empty), driver);
                //    }

                //    decimal limitRate;
                //    string limitRateElementValue = guimapadapter.GetTagTypeValue(Constants.ObOpenTradesLimitRate);
                //    if (sl.CheckIfElementFind(row, (By)sl.GetWebElement(1, limitRateElementValue, string.Empty), driver))
                //    {
                //        string limitRateString =
                //            row.FindElement((By)sl.GetWebElement(1, limitRateElementValue, string.Empty)).Text;
                //        Char[] limitRatearr =
                //            limitRateString.ToCharArray().Where(c => Char.IsDigit(c) || Char.IsPunctuation(c)).ToArray();
                //        limitRate = Convert.ToDecimal(new string(limitRatearr));

                //        stockStatus = new StockStatus(); //non stock

                //    }
                //    else
                //    {
                //        limitRate = 999999.9m;
                //    }
                //    decimal stopRate;
                //    string stopRateElementValue = guimapadapter.GetTagTypeValue(Constants.ObOpenTradeStopRate);
                //    if (sl.CheckIfElementFind(row, (By)sl.GetWebElement(1, stopRateElementValue, string.Empty),
                //                           driver))
                //    {
                //        string stopRateString =
                //            row.FindElement((By)sl.GetWebElement(1, stopRateElementValue, string.Empty)).Text;
                //        Char[] stopRatearr =
                //            stopRateString.ToCharArray().Where(c => Char.IsDigit(c) || Char.IsPunctuation(c)).ToArray();
                //        stopRate = Convert.ToDecimal(new string(stopRatearr));
                //    }
                //    else
                //    {
                //        stopRate = 0;
                //    }


                //    string instrumentElementValue = guimapadapter.GetTagTypeValue(Constants.ObOpenTradesInstrument);
                //    int positionId =
                //        Convert.ToInt32(
                //            row.FindElement((By)sl.GetWebElement(1, instrumentElementValue, string.Empty))
                //               .GetAttribute("positionid"));

                //    string infoRowPendingElementValue =
                //        guimapadapter.GetTagTypeValue(Constants.ObOpenTaredsInfoRowPending);
                //    bool isInfoRowPending = sl.CheckIfElementFind(row,
                //                                               (By)
                //                                               sl.GetWebElement(1, infoRowPendingElementValue, string.Empty),
                //                                               driver);
                //    if (isInfoRowPending)
                //    {
                //        stockStatus = StockStatus.Order;
                //    }

                //    try
                //    {
                //        openbookpositiondata.Add(new OpenBookPositionData(userName, parentUserName, initRate, instrumentDisplayName, isBuy, isOverWeekend, limitRate, stopRate, positionId, stockStatus));
                //    }
                //    catch (Exception exception)
                //    {

                //        LogObject loggerException = new LogObject();
                //        loggerException.Description = "Can't Add + " + instrumentDisplayName + " to openbook position data" + exception.Message;
                //        loggerException.StatusTag = Constants.ERROR;
                //        loggerException.Exception = exception;
                //        logger.Print(loggerException);

                //    }

                return pos ;
                }

           
            
            catch (Exception)
            {

                return null;
            }

        }
        public ValidationResponse ValidateClientOpenBookOpenTrades(string userName, List<OpenBookPositionData> uiBasedPositionList,
                                                     int flowRunId, int flowId, string flowName, int testID,
                                                     string testName)
        {
            var validationResponseresult = new ValidationResponse();
            validationResponseresult.Successful = true;
      //      var stockList = _dataAccess.GetStockListData(1945605);
            
            List<OpenBookPositionData> dbiPositionListData = _dataAccess.getOpenBookPositionDataList(userName);
            //Verify both position lists are the same size
            if (dbiPositionListData != null)
            {
                if (dbiPositionListData.Count != uiBasedPositionList.Count)
                {
                    List<int> posListDb = dbiPositionListData.Select(d => d.PositionID).ToList();

                    foreach (int y in posListDb)
                    {
                        if (!uiBasedPositionList.Exists(p => p.PositionID == y))
                        {
                            //return p.PositionID dosen't exists 
                           
                            validationResponseresult.Successful = false;
                            validationResponseresult.Information = validationResponseresult.Information+ " OpenTrades pos# -" +
                                               y + " Instrument  : "+ dbiPositionListData.Find(x => x.PositionID == y).InstrumentDisplayName+" was not found in OB \n";
                            
                        }
                    }

                    List<int> posListOb = uiBasedPositionList.Select(d => d.PositionID).ToList();
                    //Validate what missin in UI
                    foreach (int y in posListOb)
                    {
                        if (!dbiPositionListData.Exists(p => p.PositionID == y))
                        {
                            //return p.PositionID dosen't exists 

                            validationResponseresult.Successful = false;
                            validationResponseresult.Information = validationResponseresult.Information + " Open Trades pos# -" +
                                               y + " Instrument : " + uiBasedPositionList.Find(x => x.PositionID == y).InstrumentDisplayName + " was not found in DB \n";

                        }
                    }

                   

                    return validationResponseresult;
                }
            }

            //Validate what is missing in DB
           
            for (int i = 0; i < dbiPositionListData.Count; i++)
            {
                OpenBookPositionData dbFetchedPosObj =
                    dbiPositionListData.SingleOrDefault(pos => pos.PositionID == uiBasedPositionList.ElementAt(i).PositionID);
                if (dbFetchedPosObj != null)
                {
                    TestFlowInfo tfi = new TestFlowInfo(true, flowName);
                    if (!Comparator.AreObjectsEqual(dbFetchedPosObj, uiBasedPositionList.ElementAt(i), ref tfi, 0, "Pos. ID#" + dbiPositionListData.ElementAt(i).PositionID))
                         //!Comparator.AreObjectsEqual(dbFetchedPosObj, uiBasedPositionList.ElementAt(i), flowRunId, flowId,flowName, testID, testName, 0,"Pos. ID#" + dbiPositionListData.ElementAt(i).PositionID))
                    {
                        validationResponseresult.Successful = false;
                        validationResponseresult.Information = "Pos. ID#" + dbiPositionListData.ElementAt(i).PositionID;

                    }
                }
                else
                {
                    validationResponseresult.Successful = false;
                    validationResponseresult.Information = "ValidateClientPositionList failiure pos# -" +
                                               uiBasedPositionList.ElementAt(i).PositionID + " was not found in DB ";
                   
                   
                }
            }

            return validationResponseresult;
        }
    }
}