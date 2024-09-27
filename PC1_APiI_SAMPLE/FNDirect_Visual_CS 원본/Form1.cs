using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AxFNDirectLib;


namespace FNDirectTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // FNDirect.ocx wrapper 클래스 생성
            m_fndirect = new AxFNDirectLib.AxFNDirect();
            m_fndirect.CreateControl();
            m_fndirect.BeginInit();

            // 이벤트 Delegate 등록
            m_fndirect.OnReceiveProc += new _DFNDirectEvents_OnReceiveProcEventHandler(this.FnDirect_OnReceiveProc);
            m_fndirect.OnReceiveRealData += new _DFNDirectEvents_OnReceiveRealDataEventHandler(this.FnDirect_OnReceiveRealData);
            m_fndirect.OnReceiveError += new _DFNDirectEvents_OnReceiveErrorEventHandler(this.FnDirect_OnReceiveError);

            m_bConnect = false;
            m_bLogin = false;
            m_nListSel = -1;

            // 리스트 설정
            SetList();

            IDTextBox.Text = "";
            PWTextBox.Text = "";
            CerPWTextBox.Text = "";
        }

        // Form 종료 이벤트
        private void Form1Closed(object sender, FormClosedEventArgs e)
        {
            if (m_bConnect && m_bLogin)
            {
                string strUserID = IDOutTextBox.Text;
                m_fndirect.CommLogout(strUserID);

                DisplayProcTextBox("Complete Logout");

                m_fndirect.CommTerminate(1);
                m_bConnect = false;
                DisplayProcTextBox("Serer Disconnected");
            }
            else if (m_bConnect )
            {
                m_fndirect.CommTerminate(1);
                m_bConnect = false;
                DisplayProcTextBox("Serer Disconnected");
            }

            //  FNDirect.ocx 종료
            m_fndirect.Dispose();
        }

        // FNDirect 함수 요청 후 응답 이벤트
        public void FnDirect_OnReceiveProc(object sender, _DFNDirectEvents_OnReceiveProcEvent e)
        {
            string sMsg;
            sMsg = string.Format("-> [{0}:{1}] ReceiveProc", e.sProc, e.nRqID);
            DisplayProcTextBox(sMsg);

            // Item 과 Data 정보를 얻어 옴
            object arrItem = null;
            object arrData = null;

            //m_fndirect.GetProcData(e.nRqID, e.sProc, 0, ref arrItem, ref arrData);

            // 아이템정보를 얻어옴 ( FNDirect 는 VC로 만들어서.. arr를 Variant 형식으로 만들었음)
            m_fndirect.GetProcItemArr(e.nRqID, e.sProc, 0, ref arrItem);
            // OutRec 데이터를 얻어옴
            m_fndirect.ReceiveProcDataArr(e.nRqID, e.sProc, 0, ref arrData);

            //Console.WriteLine(arrData.GetType());

            // Item 은 1차원 배열
            object[] obItem = (object[])arrItem;
            // Data 는 2차원 배열
            object[,] obData = (object[,])arrData;

            string sItem = "";
            List<string> itemList = new List<string>();
            for (int i = 0; i < obItem.Length; i++)
            {
                sItem = (string)obItem[i];
                itemList.Add(sItem);
            }
       
            string sData = "";
            string sTemp = "";
            sMsg = "";

            int row = obData.GetLength(0);
            int col = obData.GetLength(1);
            
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sData = (string)obData[i, j];

                    if (itemList.Count == col)
                    {
                        sTemp = string.Format("({0}){1}", itemList[j], sData);
                        sMsg = sMsg + "|" + sTemp;
                    }
                }
                DisplayProcTextBox(sMsg);
                sMsg = "";
            }

            // OutRec 의 데이터의 갯수를 얻어옴
            long nOutRecCnt = m_fndirect.ReceiveProcOutRecCnt(e.nRqID, e.sProc, 0);
            sMsg = string.Format("OutRec1 Data Count : {0}", nOutRecCnt);
            DisplayProcTextBox(sMsg);

            if (itemList.Count > 0)
            {
                // OutRec의 특정 아이템의 데이터를 얻어옴
                sData = m_fndirect.ReceiveProc(e.nRqID, e.sProc, 0, 0, itemList[0]);
                sMsg = string.Format("{0} : {1}", itemList[0], sData);
                DisplayProcTextBox(sMsg);
            }

            //============================== s
            // OutRec2 처리
            if (e.sProc.Equals("GoDepositStat") || e.sProc.Equals("KoCandleRequest") || e.sProc.Equals("GoCandleRequest") || e.sProc.Equals("BankDollarQtRequest"))
            {
                //MessageBox.Show(e.sProc);
                // Item 과 Data 정보를 얻어 옴
                object arrItem2 = null;
                object arrData2 = null;
                                
                // 아이템정보를 얻어옴 ( FNDirect 는 VC로 만들어서.. arr를 Variant 형식으로 만들었음)
                m_fndirect.GetProcItemArr(e.nRqID, e.sProc, 1, ref arrItem2);
                // OutRec 데이터를 얻어옴
                m_fndirect.ReceiveProcDataArr(e.nRqID, e.sProc, 1, ref arrData2);                
                
                // Item 은 1차원 배열
                object[] obItem2 = (object[])arrItem2;
                // Data 는 2차원 배열
                object[,] obData2 = (object[,])arrData2;
                sItem = "";
                List<string> itemList2 = new List<string>();
                for (int i = 0; i < obItem2.Length; i++)
                {
                    sItem = (string)obItem2[i];
                    itemList2.Add(sItem);
                }

                sData = "";
                sTemp = "";
                sMsg = "";
                row = obData2.GetLength(0);
                col = obData2.GetLength(1);
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        sData = (string)obData2[i, j];

                        if (itemList2.Count == col)
                        {
                            sTemp = string.Format("({0}){1}", itemList2[j], sData);
                            sMsg = sMsg + "|" + sTemp;
                        }
                    }
                    DisplayProcTextBox(sMsg);
                    sMsg = "";
                }
            }
            //============================== e

            //============================== s
            // OutRec3 처리
            if (e.sProc.Equals("BankDollarQtRequest"))
            {
                //MessageBox.Show(e.sProc);
                // Item 과 Data 정보를 얻어 옴
                object arrItem2 = null;
                object arrData2 = null;

                // 아이템정보를 얻어옴 ( FNDirect 는 VC로 만들어서.. arr를 Variant 형식으로 만들었음)
                m_fndirect.GetProcItemArr(e.nRqID, e.sProc, 2, ref arrItem2);
                // OutRec 데이터를 얻어옴
                m_fndirect.ReceiveProcDataArr(e.nRqID, e.sProc, 2, ref arrData2);

                // Item 은 1차원 배열
                object[] obItem2 = (object[])arrItem2;
                // Data 는 2차원 배열
                object[,] obData2 = (object[,])arrData2;
                sItem = "";
                List<string> itemList2 = new List<string>();
                for (int i = 0; i < obItem2.Length; i++)
                {
                    sItem = (string)obItem2[i];
                    itemList2.Add(sItem);
                }

                sData = "";
                sTemp = "";
                sMsg = "";
                row = obData2.GetLength(0);
                col = obData2.GetLength(1);
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        sData = (string)obData2[i, j];

                        if (itemList2.Count == col)
                        {
                            sTemp = string.Format("({0}){1}", itemList2[j], sData);
                            sMsg = sMsg + "|" + sTemp;
                        }
                    }
                    DisplayProcTextBox(sMsg);
                    sMsg = "";
                }
            }
            //============================== e


        }

        // FNDirect 실시간 응답 이벤트
        public void FnDirect_OnReceiveRealData(object sender, _DFNDirectEvents_OnReceiveRealDataEvent e)
        {
            // LPCTSTR sReal : 실시간 명칭
            // sReal 값
            // "KOCASTORDE"	   국내주문 - 주문확인
            // "KOCASTMODI"	   국내주문 - 정정확인
            // "KOCASTCANC"	   국내주문 - 취소확인
            // "KOCASTFILL"	   국내주문 - 체결통보
            // "GOCASTPEND"	   해외주문 - 주문대기
            // "GOCASTORDE"	   해외주문 - 주문확인
            // "GOCASTMODI"	   해외주문 - 취소확인
            // "GOCASTCANC"	   해외주문 - 정정확인
            // "GOCASTFILL"	   해외주문 - 체결통보
            // "CMCASTORDE"	   KOSPI야간주문 - 주문확인
            // "CMCASTMODI"	   KOSPI야간주문 - 정정확인
            // "CMCASTCANC"	   KOSPI야간주문 - 취소확인
            // "CMCASTFILL"	   KOSPI야간주문 - 체결통보
            // "KOSISEDEAL"	   시세조회 - 국내체결통보 (KoPriceQtRequest 요청 후)
            // "KOSISEHOGA"	   시세조회 - 국내호가통보 (KoPriceQtRequest 요청 후)
            // "GOSISEDEAL"	   시세조회 - 해외체결통보 (GoPriceQtRequest 요청 후)
            // "GOSISEHOGA"	   시세조회 - 해외호가통보 (GoPriceQtRequest 요청 후)
            // "CMSISEDEAL"	   시세조회 - 야간체결통보 (CmPriceQtRequest 요청 후)
            // "CMSISEHOGA"	   시세조회 - 야간호가통보 (CmPriceQtRequest 요청 후)

            string sMsg;
            sMsg = string.Format("-> [{0}] Receive Real", e.sReal);
            DisplayRealTextBox(sMsg);

            object arrItem = null;
            object arrData = null;
            //m_fndirect.GetRealData(e.sReal, &varArrItem, &varArrData);
            // 리얼 아이템 정보를 얻어옴 
            m_fndirect.GetRealItemArr(e.sReal, ref arrItem);
            // 리얼 데이터 정보를 얻어옴
            m_fndirect.ReceiveRealDataArr(e.sReal, ref arrData);

            object[] obItem = (object[])arrItem;
            object[] obData = (object[])arrData;

            string sItem = "";
            List<string> itemList = new List<string>();
            for (int i = 0; i < obItem.Length; i++)
            {
                sItem = (string)obItem[i];
                itemList.Add(sItem);
            }

            string sData = "";
            string sTemp = "";
            sMsg = "";
            
            for (int i = 0; i < obData.Length; i++)
            {
                sData = (string)obData[i];  
                    
                if (itemList.Count == obData.Length)
                {
                    sTemp = string.Format("({0}){1}", itemList[i], sData);
                    sMsg = sMsg + "|" + sTemp;
                }
            }
            DisplayRealTextBox(sMsg);

            if (itemList.Count > 0)
            {
                // 아이템명으로 데이터를 얻음
                sData = m_fndirect.ReceiveReal(e.sReal, itemList[0]);
                sMsg = string.Format("{0} : {1}", itemList[0], sData);
                DisplayRealTextBox(sMsg);
            }
        }

        // 에러발생시 발생하는 이벤트
        public void FnDirect_OnReceiveError(object sender, _DFNDirectEvents_OnReceiveErrorEvent e)
        {
            string sMsg;
            sMsg = string.Format("-> [{0}:{1}] Receive Error : {2}", e.sProc, e.nRqID, e.sErrMsg);
            DisplayProcTextBox(sMsg);

            // 소캣 종료됨
            if (e.nRqID == -9999)
            {
                if (m_bLogin)
                {
                    string strUserID = IDOutTextBox.Text;

                    // 로그아웃 요청
                    m_fndirect.CommLogout(strUserID);
                    m_bLogin = false;
                }

                if (m_bConnect)
                    DiConnectbutton_Click(null, null);
            }
        }


        private void Connectbutton_Click(object sender, EventArgs e)
        {
            if (!m_bConnect)
            {
              // 서버접속 요청
              int nRes = m_fndirect.CommInit();
              if (nRes == 1)
              {
                  m_bConnect = true;
                  DisplayProcTextBox("Serer Connected");
              }
              else
                  DisplayProcTextBox("Failed Serer");
            }
        }

        private void DiConnectbutton_Click(object sender, EventArgs e)
        {
            if (m_bConnect)
            {
                // 서버접속 종료요청
                m_fndirect.CommTerminate(1);
                m_bConnect = false;
                DisplayProcTextBox("Serer Disconnected");
            }
        }

        private void Loginbutton_Click(object sender, EventArgs e)
        {
            if (!m_bConnect)
                Connectbutton_Click(null, null);

            if (m_bConnect)
            {
                string strUserID = IDTextBox.Text;
                string strPasswd = PWTextBox.Text;
                string strCertPasswd = CerPWTextBox.Text;

                // 로그인 요청
                int nRes = m_fndirect.CommLogin(strUserID, strPasswd, strCertPasswd);
                if (nRes == 1)
                {
                    m_bLogin = true;
                    IDOutTextBox.Text = strUserID;
                    DisplayProcTextBox("Complete Login");
                }
                else
                {
                    DisplayProcTextBox("Failed Login");
                    // 로그인 실패시 서버 접속 종료
                    DiConnectbutton_Click(null, null);
                }
            }
        }

        private void Logoutbutton_Click(object sender, EventArgs e)
        {
            if (m_bConnect && m_bLogin)
            {
                string strUserID = IDOutTextBox.Text;

                // 로그아웃 요청
                m_fndirect.CommLogout(strUserID);
                m_bLogin = false;

                DisplayProcTextBox("Complete Logout");

                // 로그아웃 후 서버 접속 종료
                DiConnectbutton_Click(null, null);
            }
        }

        private void DisplayProcTextBox(string sMsg)
        {
            string strAdd = sMsg + "\r\n";
            PorcTextBox.AppendText(strAdd);
            PorcTextBox.ScrollToCaret();            
        }

        private void DisplayRealTextBox(string sMsg)
        {
            string strAdd = sMsg + "\r\n";
            RealTextBox.AppendText(strAdd);
            RealTextBox.ScrollToCaret();  
        }

        
        // 리스트에 표시할 정보
        public struct PROCINFO
        {
            public string sTitle;
            public bool bArr;
            public int nInputCnt;
           

            public PROCINFO(string sTitle, bool bArr, int nInputCnt)
            {
                this.sTitle = sTitle;
                this.bArr = bArr;
                this.nInputCnt = nInputCnt;           
            }
        }

        public PROCINFO[] procinfo = {
                new PROCINFO("국내주문-KoOrderSend(다중입력가능)",		    true,	8), 
	            new PROCINFO("국내주문취소-KoCancelSend(다중입력가능)",	    true,	4), 
	            new PROCINFO("국내주문정정-KoModifySend(다중입력가능)",	    true,	5), 
	            new PROCINFO("국내주문미체결조회-KoWorkingOrder",			    false,	2),
	            new PROCINFO("국내주문미체결조회연속조회-KoWorkingOrderNext", false,	0),
	            new PROCINFO("국내주문체결조회-KoFilledList",				    false,	2),
	            new PROCINFO("국내주문체결조회연속조회-KoFilledListNext",	    false,	0),
	            new PROCINFO("국내주문내역-KoOrderList",					    false,	2),
	            new PROCINFO("국내주문내역연속조회-KoOrderListNext",			false,	0),
	            new PROCINFO("국내주문미결제약정조회-KoOpenInterest",		    false,	3),
	            new PROCINFO("국내주문미결제약정연속조회-KoOpenInterestNext", false,	0),
	            new PROCINFO("국내주문위탁계좌예수금조회-KoDepositStat",		false,	2),
	            new PROCINFO("국내주문가능계좌조회-KoAcountList",			    false,	0),
	            new PROCINFO("국내주문가능수량조회-KoOrdableQty",			    false,	5),
	            new PROCINFO("해외주문-GoOrderSend",						    false, 10),
	            new PROCINFO("해외주문취소-GoCancelSend",					false,	3),
	            new PROCINFO("해외주문정정-GoModifySend",					false,	7),
	            new PROCINFO("해외주문미체결조회-GoWorkingOrder",			    false,	2),
                new PROCINFO("해외주문미체결연속조회-GoWorkingOrderNext",	    false,	0),
	            new PROCINFO("해외주문체결조회-GoOrderFillList",				false,	5),
	            new PROCINFO("해외주문체결연속조회-GoOrderFillListNext",		false,	0),
                new PROCINFO("해외주문체결조회(기간)-GoOrderPeriodList",			false,	4),
	            new PROCINFO("해외주문체결연속조회(기간)-GoOrderPeriodListNext",	false,	0),
	            new PROCINFO("해외주문미결제약정현황-GoOpenInterest",		    false,	3),
	            new PROCINFO("해외주문예수금현황-GoDepositStat",				false,	4),
	            new PROCINFO("해외주문가능계좌조회-GoAcountList",			    false,	0),
	            new PROCINFO("해외주문통화코드리스트조회-GoCurrencyCode",	    false,	1),
	            new PROCINFO("해외주문가능수량조회-GoOrdableQty",			    false,	4),
	            new PROCINFO("KOSPI야간주문-CmOrderSend(다중입력가능)",		true,	8),
	            new PROCINFO("KOSPI야간주문취소-CmCancelSend(다중입력가능)",	true,	4),
	            new PROCINFO("KOSPI야간주문정정-CmModifySend(다중입력가능)",	true,	7),
	            new PROCINFO("KOSPI야간주문미체결조회-CmWorkingOrder",		false,	2),
	            new PROCINFO("KOSPI야간주문미체결연속조회-CmWorkingOrderNext",false,	0),
	            new PROCINFO("KOSPI야간주문체결조회-CmFilledList",			false,	4),
	            new PROCINFO("KOSPI야간주문체결연속조회-CmFilledListNext",	false,	0),
	            new PROCINFO("KOSPI야간주문미결제약정조회-CmOpenInterest",	false,	3),
	            new PROCINFO("KOSPI야간주문미결제약정연속조회-CmOpenInterestNext",false,	0),
	            new PROCINFO("KOSPI야간주문위탁계좌예수금조회-CmDepositStat", false,	2),
	            new PROCINFO("KOSPI야간주문CME주문가능수량조회-CmOrdableQty", false,	5),
	            new PROCINFO("국내시세-KoPriceQtRequest(다중입력가능)",		true,	1),
	            new PROCINFO("해외시세-GoPriceQtRequest(다중입력가능)",		true,	1),
	            new PROCINFO("CME시세-CmPriceQtRequest(다중입력가능)",		true,	1),
	            new PROCINFO("국내시세중지-KoPriceQtRequest(다중입력가능)",	true,	1),
	            new PROCINFO("해외시세중지-GoPriceQtRequest(다중입력가능)",	true,	1),
	            new PROCINFO("CME시세중지-CmPriceQtRequest(다중입력가능)",	true,	1),
                new PROCINFO("국내선물 마스터 요청-SeriesInfoRequest",		false,	1),
                new PROCINFO("국내옵션 마스터 요청-SeriesInfoRequest",		false,	1),
                new PROCINFO("국내스프레드 마스터 요청-SeriesInfoRequest",	false,	1),
                new PROCINFO("해외선물 마스터 요청-SeriesInfoRequest",		false,	1),
                new PROCINFO("국내주식차트조회-KoCandleRequest",		        false,	13),
                new PROCINFO("해외주식차트조회-GoCandleRequest",	        	false,	14),
                new PROCINFO("은행달러 시세요청-BankDollarQtRequest",   	    false,	0),
                new PROCINFO("은행달러 시세중지-BankDollarQtRequest",	        false,	0)
            };


        // 리스트에 표시
        private void SetList()
        {  
            ProclistView.Columns.Add("Proc", 350, HorizontalAlignment.Left);

            foreach (PROCINFO proc in procinfo)
            {
                ListViewItem item = new ListViewItem(proc.sTitle);
                ProclistView.Items.Add(item);
            }
        }

        // 리스트 선택 이벤트
        private void ProcListSelectChange(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection listSelIndex = this.ProclistView.SelectedIndices;
            if (listSelIndex.Count != 0)
            {
                foreach (int index in listSelIndex)
                    m_nListSel = index;
            }

            SetGrid();
        }

        // PropertyGrid 표시
        private void SetGrid()
        {
            switch (m_nListSel)
            {
                case 0:
                case 28:
                    PropertyGrid_0 grid = new PropertyGrid_0();
                    InputGrid.SelectedObject = grid;
                    break;
                case 1:
                    PropertyGrid_1 grid1 = new PropertyGrid_1();
                    InputGrid.SelectedObject = grid1;
                    break;
                case 2:
                    PropertyGrid_2 grid2 = new PropertyGrid_2();
                    InputGrid.SelectedObject = grid2;
                    break;
                case 3:
                case 5:
                case 7:
                case 11:
                case 17:
                case 31:
                case 37:
                    PropertyGrid_3 grid3 = new PropertyGrid_3();
                    InputGrid.SelectedObject = grid3;
                    break;
                case 9:
                case 23:
                case 35:
                    PropertyGrid_4 grid4 = new PropertyGrid_4();
                    InputGrid.SelectedObject = grid4;
                    break;
                case 13:
                case 38:
                    PropertyGrid_5 grid5 = new PropertyGrid_5();
                    InputGrid.SelectedObject = grid5;
                    break;
                case 14:
                    PropertyGrid_6 grid6 = new PropertyGrid_6();
                    InputGrid.SelectedObject = grid6;
                    break;
                case 15:
                    PropertyGrid_50 grid50 = new PropertyGrid_50();
                    InputGrid.SelectedObject = grid50;
                    break;
                case 16:
                    PropertyGrid_7 grid7 = new PropertyGrid_7();
                    InputGrid.SelectedObject = grid7;
                    break;
                case 19:
                    PropertyGrid_8 grid8 = new PropertyGrid_8();
                    InputGrid.SelectedObject = grid8;
                    break;
                case 21:
                    PropertyGrid_20 grid23 = new PropertyGrid_20();     // 해외주문 주문체결 조회(기간)
                    InputGrid.SelectedObject = grid23;
                    break;
                case 24:
                    PropertyGrid_9 grid9 = new PropertyGrid_9();
                    InputGrid.SelectedObject = grid9;
                    break;
                case 26:
                    PropertyGrid_10 grid10 = new PropertyGrid_10();
                    InputGrid.SelectedObject = grid10;
                    break;
                case 27:
                    PropertyGrid_11 grid11 = new PropertyGrid_11();
                    InputGrid.SelectedObject = grid11;
                    break;
                case 29:
                    PropertyGrid_12 grid12 = new PropertyGrid_12();
                    InputGrid.SelectedObject = grid12;
                    break;
                case 30:
                    PropertyGrid_13 grid13 = new PropertyGrid_13();
                    InputGrid.SelectedObject = grid13;
                    break;
                case 33:
                    PropertyGrid_14 grid14 = new PropertyGrid_14();
                    InputGrid.SelectedObject = grid14;
                    break;
                case 39:
                case 41:
                case 42:
                case 44:
                    PropertyGrid_15 grid15 = new PropertyGrid_15();
                    InputGrid.SelectedObject = grid15;
                    break;
                case 40:
                case 43:
                    PropertyGrid_16 grid16 = new PropertyGrid_16();
                    InputGrid.SelectedObject = grid16;
                    break;
                case 45:
                    PropertyGrid_17 grid17 = new PropertyGrid_17();
                    InputGrid.SelectedObject = grid17;
                    break;
                case 46:
                    PropertyGrid_17 grid18 = new PropertyGrid_17();
                    grid18._Type = "2";
                    InputGrid.SelectedObject = grid18;
                    break;
                case 47:
                    PropertyGrid_17 grid19 = new PropertyGrid_17();
                    grid19._Type = "3";
                    InputGrid.SelectedObject = grid19;
                    break;
                case 48:
                    PropertyGrid_17 grid20 = new PropertyGrid_17();
                    grid20._Type = "4";
                    InputGrid.SelectedObject = grid20;
                    break;
                case 49:
                    PropertyGrid_18 grid21 = new PropertyGrid_18();
                    InputGrid.SelectedObject = grid21;
                    break;
                case 50:
                    PropertyGrid_19 grid22 = new PropertyGrid_19();
                    InputGrid.SelectedObject = grid22;
                    break;               

                default:
                    InputGrid.SelectedObject = null;
                    break;
            }

        }

        // 요청 버튼 클릭 
        private void Procbutton_Click(object sender, EventArgs e)
        {           
            string sInputData = "";
            int nType = 0;
            if (InputGrid.SelectedObject != null)
            {
                object ob = InputGrid.SelectedObject;
                 string sInput = "";
                
                switch (m_nListSel)
                {
                    case 0:
                    case 28:
                        PropertyGrid_0 grid = (PropertyGrid_0)ob;
                        sInput = grid.GetInputData();
                        break;
                    case 1:   
                        PropertyGrid_1 grid1 = (PropertyGrid_1)ob;
                        sInput = grid1.GetInputData();
                        break;
                    case 2:
                        PropertyGrid_2 grid2 = (PropertyGrid_2)ob;
                        sInput = grid2.GetInputData();
                        break;
                    case 3:
                    case 5:
                    case 7:
                    case 11:
                    case 17:
                    case 31:
                    case 37:
                        PropertyGrid_3 grid3 = (PropertyGrid_3)ob;
                        sInput = grid3.GetInputData();
                        break;
                    case 9:
                    case 23:
                    case 35:
                        PropertyGrid_4 grid4 = (PropertyGrid_4)ob;
                        sInput = grid4.GetInputData();
                        break;
                    case 13:
                    case 38:
                        PropertyGrid_5 grid5 = (PropertyGrid_5)ob;
                        sInput = grid5.GetInputData();
                        break;
                    case 14:
                        PropertyGrid_6 grid6 = (PropertyGrid_6)ob;
                        sInput = grid6.GetInputData();
                        break;
                    case 15:
                        PropertyGrid_50 grid50 = (PropertyGrid_50)ob;
                        sInput = grid50.GetInputData();
                        break;
                    case 16:
                        PropertyGrid_7 grid7 = (PropertyGrid_7)ob;
                        sInput = grid7.GetInputData();
                        break;
                    case 19:
                        PropertyGrid_8 grid8 = (PropertyGrid_8)ob;
                        sInput = grid8.GetInputData();
                        break;
                    case 21:
                        PropertyGrid_20 grid20 = (PropertyGrid_20)ob;
                        sInput = grid20.GetInputData();
                        break;
                    case 24:
                        PropertyGrid_9 grid9 = (PropertyGrid_9)ob;
                        sInput = grid9.GetInputData();
                        break;
                    case 26:
                        PropertyGrid_10 grid10 = (PropertyGrid_10)ob;
                        sInput = grid10.GetInputData();
                        break;
                    case 27:
                        PropertyGrid_11 grid11 = (PropertyGrid_11)ob;
                        sInput = grid11.GetInputData();
                        break;
                    case 29:
                        PropertyGrid_12 grid12 = (PropertyGrid_12)ob;
                        sInput = grid12.GetInputData();
                        break;
                    case 30:
                        PropertyGrid_13 grid13 = (PropertyGrid_13)ob;
                        sInput = grid13.GetInputData();
                        break;
                    case 33:
                        PropertyGrid_14 grid14 = (PropertyGrid_14)ob;
                        sInput = grid14.GetInputData();
                        break;
                    case 39:
                    case 41:
                    case 42:
                    case 44:
                        PropertyGrid_15 grid15 = (PropertyGrid_15)ob;
                        sInput = grid15.GetInputData();
                        break;
                    case 40:
                    case 43:
                        PropertyGrid_16 grid16 = (PropertyGrid_16)ob;
                        sInput = grid16.GetInputData();
                        break;
                    case 45:
                    case 46:
                    case 47:
                    case 48:
                        PropertyGrid_17 grid17 = (PropertyGrid_17)ob;
                        sInput = grid17.GetInputData();
                        nType = (int)Convert.ToUInt32(sInput);                        
                        break;
                    case 49:
                        PropertyGrid_18 grid18 = (PropertyGrid_18)ob;
                        sInput = grid18.GetInputData();
                        break;
                    case 50:
                        PropertyGrid_19 grid19 = (PropertyGrid_19)ob;
                        sInput = grid19.GetInputData();
                        break;
                    default:
                        break;
                }
                                

                // OCX 넘겨야 하는 데이터는 멀티바이트이어야 함.
                IntPtr ptrData = Marshal.StringToHGlobalAnsi(sInput);
                string strTemp = Marshal.PtrToStringAnsi(ptrData, sInput.Length);
                sInputData = strTemp.TrimEnd();
                Marshal.FreeHGlobal(ptrData);
            }

            CallProc(sInputData, nType);
        }

        // FNDirect 함수 호출
        private void CallProc(string sInputData, int nType)
        {
	        int nRqID = 0;
	        switch(m_nListSel)
	        {
	            case 0:
                    nRqID = m_fndirect.KoOrderSend(sInputData);     break;      // 국내주문
	            case 1:
                    nRqID = m_fndirect.KoCancelSend(sInputData);    break;      // 국내주문 취소
	            case 2:
		            nRqID = m_fndirect.KoModifySend(sInputData);	break;      // 국내주문 정정
	            case 3:
		            nRqID = m_fndirect.KoWorkingOrder(sInputData);	break;      // 국내주문 미체결 조회
	            case 4:
		            nRqID = m_fndirect.KoWorkingOrderNext();		break;      // 국내주문 미체결 연속조회
	            case 5:
		            nRqID = m_fndirect.KoFilledList(sInputData);	break;      // 국내주문 체결 조회
	            case 6:
		            nRqID = m_fndirect.KoFilledListNext();			break;      // 국내주문 체결 연속조회
	            case 7:
		            nRqID = m_fndirect.KoOrderList(sInputData);		break;      // 국내주문 주문내역 조회
	            case 8:
		            nRqID = m_fndirect.KoOrderListNext();			break;      // 국내주문 주문내역 연속조회
	            case 9:
		            nRqID = m_fndirect.KoOpenInterest(sInputData);	break;      // 국내주문 미결제약정 조회
	            case 10:
		            nRqID = m_fndirect.KoOpenInterestNext();		break;      // 국내주문 미결제약정 연속조회
	            case 11:
		            nRqID = m_fndirect.KoDepositStat(sInputData);	break;      // 국내주문 위탁계좌예수금 조회
	            case 12:
		            nRqID = m_fndirect.KoAcountList();				break;      // 국내주문 국내주문가능계좌조회
	            case 13:
		            nRqID = m_fndirect.KoOrdableQty(sInputData);	break;      // 국내주문 가능수량 조회
	            case 14:
		            nRqID = m_fndirect.GoOrderSend(sInputData);		break;      // 해외주문
                case 15:
                    nRqID = m_fndirect.GoCancelSend(sInputData);    break;      // 해외주문 취소
                case 16:
		            nRqID = m_fndirect.GoModifySend(sInputData);	break;      // 해외주문 정정
                case 17:
		            nRqID = m_fndirect.GoWorkingOrder(sInputData);	break;      // 해외주문 미체결 조회
	            case 18:
		            nRqID = m_fndirect.GoWorkingOrderNext();		break;      // 해외주문 미체결 연속조회
	            case 19:
                    nRqID = m_fndirect.GoOrderFillList(sInputData); break;      // 해외주문 주문체결 조회
	            case 20:
		            nRqID = m_fndirect.GoOrderFillListNext();		break;      // 해외주문 주문체결 연속조회
                case 21:
                    nRqID = m_fndirect.GoOrderPeriodList(sInputData); break;    // 해외주문 주문체결 조회(기간)
                case 22:
                    nRqID = m_fndirect.GoOrderPeriodListNext();     break;      // 해외주문 주문체결 연속조회(기간)               
                case 23:
		            nRqID = m_fndirect.GoOpenInterest(sInputData);	break;      // 해외주문 미결제약정현황
	            case 24:
		            nRqID = m_fndirect.GoDepositStat(sInputData);	break;      // 해외주문 예수금현황
	            case 25:
		            nRqID = m_fndirect.GoAcountList();				break;      // 해외주문 가능계좌조회
	            case 26:
		            nRqID = m_fndirect.GoCurrencyCode(sInputData);	break;      // 해외 통화코드리스트 조회
	            case 27:
		            nRqID = m_fndirect.GoOrdableQty(sInputData);	break;      // 해외 주문가능수량 조회
	            case 28:
		            nRqID = m_fndirect.CmOrderSend(sInputData);		break;      // KOSPI야간주문
	            case 29:
		            nRqID = m_fndirect.CmCancelSend(sInputData);	break;      // KOSPI야간주문 취소
	            case 30:
		            nRqID = m_fndirect.CmModifySend(sInputData);	break;      // KOSPI야간주문 정정
	            case 31:
		            nRqID = m_fndirect.CmWorkingOrder(sInputData);	break;      // KOSPI야간주문 미체결 조회
	            case 32:
		            nRqID = m_fndirect.CmWorkingOrderNext();		break;      // KOSPI야간주문 미체결 연속조회
	            case 33:
		            nRqID = m_fndirect.CmFilledList(sInputData);	break;      // KOSPI야간주문 체결 조회
	            case 34:
		            nRqID = m_fndirect.CmFilledListNext();			break;      // KOSPI야간주문 체결 연속조회
	            case 35:
		            nRqID = m_fndirect.CmOpenInterest(sInputData);	break;      // KOSPI야간주문 미결제약정 조회
	            case 36:
		            nRqID = m_fndirect.CmOpenInterestNext();		break;      // KOSPI야간주문 미결제약정 연속조회
	            case 37:
		            nRqID = m_fndirect.CmDepositStat(sInputData);	break;      // KOSPI야간주문 위탁계좌예수금 조회
	            case 38:
		            nRqID = m_fndirect.CmOrdableQty(sInputData);	break;      // KOSPI야간주문 CME주문가능수량 조회
	            case 39:
		            nRqID = m_fndirect.KoPriceQtRequest(sInputData,1);break;    // 시세조회 - 국내시세
	            case 40:
		            nRqID = m_fndirect.GoPriceQtRequest(sInputData,1);break;    // 시세조회 - 해외시세 
	            case 41:
		            nRqID = m_fndirect.CmPriceQtRequest(sInputData,1);break;    // 시세조회 - CME시세 
	            case 42:
		            nRqID = m_fndirect.KoPriceQtRequest(sInputData,0);break;    // 시세조회중지 - 국내시세
	            case 43:
		            nRqID = m_fndirect.GoPriceQtRequest(sInputData,0);break;    // 시세조회중지 - 해외시세 
	            case 44:
		            nRqID = m_fndirect.CmPriceQtRequest(sInputData,0);break;    // 시세조회중지 - CME시세 
	            case 45:
	            case 46:
	            case 47:
	            case 48:
		            nRqID = m_fndirect.SeriesInfoRequest(nType);       break;   // 마스터 데이터 요청 함수
                case 49:
                    nRqID = m_fndirect.KoCandleRequest(sInputData); break;   
                case 50:
                    nRqID = m_fndirect.GoCandleRequest(sInputData); break;
                case 51:
                    nRqID = m_fndirect.BankDollarQtRequest(1); break;
                case 52:
                    nRqID = m_fndirect.BankDollarQtRequest(0); break;
	            default:
		            break;
	            };

	            string sMsg;
	            if(nRqID >= 0)
	            {
                    sMsg = string.Format("<- Request [{0}:{1}] : {2}", procinfo[m_nListSel].sTitle, nRqID, sInputData);		
	            }
	            else
	            {
		            string sErrType = "";
		            switch(nRqID)
		            {
		            case -1000:
			            sErrType = "입력데이터 에러"; break;
		            case -1001:
			            sErrType = "잘못된 계좌번호 입력"; break;
		            case -1002:
			            sErrType = "연속조회를 할 수 없음"; break;
		            case -1003:
			            sErrType = "이전 요청 Proc 이 응답이 오지 않은 경우(동기식)"; break;
		            case -1004:
			            sErrType = "Proc I/O 정보를 얻지 못하는 경우"; break;
                    case -1005:
                        sErrType = "주문시 인증서 에러"; break;
                    case -1006:
                        sErrType = "연속으로 서비스 요청 불가능"; break;
		            default:
			            break;
		            }

                    sMsg = string.Format("<- Error {0} [{1}:{2}] : {3} ", sErrType, procinfo[m_nListSel].sTitle, nRqID, sInputData);
	            }

                DisplayProcTextBox(sMsg);
           }
    }
}
