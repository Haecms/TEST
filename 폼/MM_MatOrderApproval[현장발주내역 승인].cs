﻿using DC00_assm;
using DC00_Component;
using Infragistics.UltraChart.Core.Layers;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;
/*************************************************************************************************************
 * 화면 ID  : MM_MatOrderApproval
 * 작성자   : 권문규
 * 작성일자 : 2023-08-08
 * 설명     : 현장발주내역 승인
 * ***********************************************************************************************************
 * 수정 이력 :
 * 
 * 
 * ***********************************************************************************************************/

namespace KDTB_FORMS
{
    public partial class MM_MatOrderApproval : DC00_WinForm.BaseMDIChildForm
    {
        UltraGridUtil _GridUtil = new UltraGridUtil(); // 그리드 셋팅 클래스
        public MM_MatOrderApproval()
        {
            InitializeComponent();
        }

        private void MM_MatOrderApproval_Load(object sender, EventArgs e)
        {
            // 1. 그리드 셋팅
            
            _GridUtil.InitializeGrid(grid1);   // 싹 비우고 시작 Clear랑 같은 의미
            _GridUtil.InitColumnUltraGrid(grid1, "CHK"      , "승인"        , GridColDataType_emu.CheckBox    , 80 , HAlign.Left  , true, true);
            _GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE", "공장"        , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "REQDATE"  , "요청일자"    , GridColDataType_emu.YearMonthDay, 130, HAlign.Center, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMCODE" , "품목"        , GridColDataType_emu.VarChar     , 100, HAlign.Left  , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "ITEMNAME" , "품명"        , GridColDataType_emu.VarChar     , 100, HAlign.Left  , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "REQQTY"   , "발주요청수량", GridColDataType_emu.Double      , 100, HAlign.Right , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "UNITCODE" , "단위"        , GridColDataType_emu.VarChar     , 100, HAlign.Center, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "CUSTNAME" , "거래처"      , GridColDataType_emu.VarChar     , 100, HAlign.Left  , true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKER"    , "발주요청자"  , GridColDataType_emu.VarChar     , 130, HAlign.Center, true, false);
            _GridUtil.InitColumnUltraGrid(grid1, "MAKEDATE" , "요청일시"    , GridColDataType_emu.DateTime24  , 130, HAlign.Left  , true, false);
            _GridUtil.SetInitUltraGridBind(grid1);

            // 콤보박스 셋팅
            DataTable dtTemp = new DataTable();

            // 공장
            dtTemp = Common.StandardCODE("PLANTCODE");                    
            Common.FillComboboxMaster(cboPlantCode, dtTemp);              
            UltraGridUtil.SetComboUltraGrid(grid1, "PLANTCODE", dtTemp);

            // 협력업체 (거래처)
            dtTemp = Common.GET_Cust_Code("V");                           
            Common.FillComboboxMaster(cboCustCode, dtTemp);               
            UltraGridUtil.SetComboUltraGrid(grid1, "CUSTCODE", dtTemp);   

            // 거래처
            dtTemp = Common.StandardCODE("UNITCODE");                     
            UltraGridUtil.SetComboUltraGrid(grid1, "UNITCODE", dtTemp);   

            // 발주 품목
            dtTemp = Common.Get_ItemCode(new string[] {"ROH"});           
            Common.FillComboboxMaster(cboItemCode, dtTemp);
            UltraGridUtil.SetComboUltraGrid(grid1, "ITEMCODE", dtTemp);

            // 입출고 일자 기본 날짜 선택
            dtpStart.Value = string.Format("{0:yyyy-MM-01}", DateTime.Now);
            dtpEnd.Value   = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
        }

        public override void DoInquire()
        {
            // 구매자재 발주 및 입고 내역 조회
            string sPlantCode  = Convert.ToString(cboPlantCode.Value);            // 공장
            string sCustCode   = Convert.ToString(cboCustCode.Value);             // 거래처
            string sItemCode   = Convert.ToString(cboItemCode.Value);             // 품목코드
            string sStartDate  = string.Format("{0:yyyy-MM-dd}", dtpStart.Value); // 발주요청 시작일자
            string sEndDate    = string.Format("{0:yyyy-MM-dd}", dtpEnd.Value);   // 발주요청 종료일자

            DBHelper helper = new DBHelper(false);
            try
            {
                _GridUtil.Grid_Clear(grid1);    // 그리드 데이터 행 삭제 
                // Database에서 작업자 정보 조회
                DataTable dtTemp = new DataTable();
                dtTemp = helper.FillTable("_1JO_MM_MatOrderApproval_S1", CommandType.StoredProcedure
                                         , helper.CreateParameter("@PLANTCODE", sPlantCode)
                                         , helper.CreateParameter("@CUSTCODE" , sCustCode)
                                         , helper.CreateParameter("@ITEMCODE" , sItemCode)
                                         , helper.CreateParameter("@STARTDATE", sStartDate)
                                         , helper.CreateParameter("@ENDDATE"  , sEndDate));
                grid1.DataSource = dtTemp;
                if (grid1.Rows.Count == 0)
                {
                    ClosePrgForm(); //프로그레스 상태 창 닫기
                    ShowDialog("조회할 데이터가 없습니다.");
                }

            }
            catch(Exception ex)
            {
                ShowDialog(ex.Message);
            }
            finally
            {
                helper.Close();
            }
        }

        public override void DoSave()
        {
            // 변경된 이력이 있는 행을 데이터 테이블에 담기
            DataTable dt = grid1.chkChange();
            if (dt == null)
            {
                ShowDialog("선택한 항목이 존재하지 않습니다. 확인 후 다시 시도해주세요.");
                return;
            }
            if (dt.Rows.Count == 0)
            {
                ShowDialog("선택한 항목이 존재하지 않습니다. 확인 후 다시 시도해주세요.");
                return;
            }

            // 승인 확인 메시지 출력
            if (ShowDialog("발주요청을 승인하시겠습니까?") == DialogResult.Cancel)
            {
                return;
            }

            DBHelper helper = new DBHelper(true);
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["CHK"]) == 0)
                    {
                        continue;
                    }

                    string sPlantcode = Convert.ToString(row["PLANTCODE"]);
                    string sItemcode  = Convert.ToString(row["ITEMCODE"]);
                    int iPOQTY        = Convert.ToInt32(row["REQQTY"]);
                    string sUnitcode  = Convert.ToString(row["UNITCODE"]);
                    string sCustname  = Convert.ToString(row["CUSTNAME"]);
                    string sMaker     = Convert.ToString(LoginInfo.UserID);
                    DateTime sDate      = Convert.ToDateTime(row["MAKEDATE"]);
                    helper.ExecuteNoneQuery("_1JO_MM_MatOrderApproval_I1", CommandType.StoredProcedure
                                           , helper.CreateParameter("@PLANTCODE", sPlantcode)
                                           , helper.CreateParameter("@ITEMCODE" , sItemcode)
                                           , helper.CreateParameter("@POQTY"    , iPOQTY)
                                           , helper.CreateParameter("@UNITCODE" , sUnitcode)
                                           , helper.CreateParameter("@CUSTNAME" , sCustname)
                                           , helper.CreateParameter("@MAKER"    , sMaker));
                    helper.ExecuteNoneQuery("_1JO_MM_MatOrderApproval_D1", CommandType.StoredProcedure
                                           , helper.CreateParameter("@DATE", sDate));
                }

                helper.Commit();
                ShowDialog("발주 승인이 완료되었습니다.");
                DoInquire();
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.Message);
            }
            finally
            {
                helper.Close();
            }
            /*
            DBHelper helper = new DBHelper(true);
            
            try
            {
                string sMessage = "";
                // 변경된 이력이 담긴 테이블에 있는 행을 하나씩 추출 
                foreach (DataRow row in dt.Rows)
                {
                    switch (row.RowState)
                    {
                        case DataRowState.Deleted:
                            // 발주 내역 취소
                            row.RejectChanges();
                            helper.ExecuteNoneQuery("SP07_MM_MatOrderApproval_D1", CommandType.StoredProcedure,
                                                    helper.CreateParameter("@PLANTCODE", Convert.ToString(row["PLANTCODE"])),
                                                    helper.CreateParameter("@PONO"     , Convert.ToString(row["PONO"])));
                            break;
                        case DataRowState.Added:
                            // 신규 발주 등록

                            if (Convert.ToString(row["ITEMCODE"]) == "") sMessage += "아이템코드 ";
                            if (Convert.ToString(row["POQTY"])    == "") sMessage += "발주수량 ";
                            if (Convert.ToString(row["CUSTCODE"]) == "") sMessage += "거래처 ";
                            if (sMessage != "")
                            {
                                helper.Rollback();
                                ShowDialog(sMessage + "을(를) 입력해주세요");
                                return;
                            }
                            helper.ExecuteNoneQuery("SP07_MM_MatOrderApproval_I1", CommandType.StoredProcedure,
                                                    helper.CreateParameter("@PLANTCODE", Convert.ToString(row["PLANTCODE"])),
                                                    helper.CreateParameter("@ITEMCODE" , Convert.ToString(row["ITEMCODE"])),
                                                    helper.CreateParameter("@PODATE"   , Convert.ToString(row["PODATE"])),
                                                    helper.CreateParameter("@POQTY"    , Convert.ToString(row["POQTY"]).Replace(",","")),
                                                    helper.CreateParameter("@UNITCODE" , Convert.ToString(row["UNITCODE"])),
                                                    helper.CreateParameter("@CUSTCODE" , Convert.ToString(row["CUSTCODE"])),
                                                    helper.CreateParameter("@MAKER"    , LoginInfo.UserID));

                            break;
                        case DataRowState.Modified:
                            // 입고 등록을 하겠다! 라는 말
                            helper.ExecuteNoneQuery("SP07_MM_MatOrderApproval_U1", CommandType.StoredProcedure,
                                                    helper.CreateParameter("@PLANTCODE", Convert.ToString(row["PLANTCODE"])),
                                                    helper.CreateParameter("@PONO"     , Convert.ToString(row["PONO"])),
                                                    helper.CreateParameter("@INQTY"    , Convert.ToString(row["INQTY"]).Replace(",", "")),
                                                    helper.CreateParameter("@INWORKER" , LoginInfo.UserID));

                            break;
                    }
                    if(helper.RSCODE != "S")
                    {
                        helper.Rollback();
                        ShowDialog($"등록중 오류가 발생하였습니다. \r\n {helper.RSMSG}");
                        return;
                    }
                }
                helper.Commit();
                ShowDialog("정상적으로 등록되었습니다.");
                DoInquire(); //등록된 데이터 재 조회.
            }
            catch (Exception ex)
            {
                helper.Rollback();
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }

        */
        }

   

    }
}
