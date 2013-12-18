<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="RequestTracker._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Request Tracker - Home</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
    <!-- Javascript needed for GetEmployees.aspx: -->
    <script type="text/javascript" src="/WebTools/EmployeeLookup/GetEmployees.js"></script>
    <script type="text/javascript" src="/WebTools/ModelNumberLookup/ObjectPositioning.js"></script>
</head>
<body onload="document.forms[0].TextBoxJobNumber.focus();">
    <form id="form1" runat="server">
        <table cellpadding="0" cellspacing="0" border="0" class="Level1" style="width:60%">
            <tr>
                <td colspan="2" class="Level1Caption">Request Tracker</td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
            <tr>
                <td class="Level1Data">
                    <table cellpadding="0" cellspacing="0" border="0" class="Level2" style="width:100%">
                        <tr>
                            <td colspan="2" class="Level2Caption">Enter a New Request</td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align:center;vertical-align:middle;height:40px" class="Level2Data">
                                <input id="Button1" type="button" value="Go" onclick="window.location.pathname='/RequestTracker/JobDetails.aspx'" style="width:60px" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
            <tr>
                <td class="Level1Data">
                    <table cellpadding="0" cellspacing="0" border="0" class="Level2" style="width:100%">
                        <tr>
                            <td colspan="2" class="Level2Caption">Search Requests</td>
                        </tr>
                        <tr><td colspan="2" style="font-size:8pt; font-weight:normal; text-align:center" class="Level2Data">(If no fields are set, all records are returned.)</td></tr>
                        <tr>
                            <td class="Level2Header" style="height: 28px">Request Number:</td>
                            <td class="Level2Data" style="height: 28px">
                                <asp:TextBox ID="TextBoxJobNumber" runat="server" Width="100px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Requestor: <span style="font-size:8pt; font-weight:normal">(last name, first name)</span></td>
                            <td class="Level2Data">
                                <asp:TextBox ID="TextBoxEmployee" runat="server" Width="250px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Technician: <span style="font-size:8pt; font-weight:normal">(last name, first name)</span></td>
                            <td class="Level2Data">
                                <asp:TextBox ID="TextBoxTechnician" runat="server" Width="250px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Status:</td>
                            <td class="Level2Data">
                                <asp:DropDownList ID="DropDownListStatus" runat="server">
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Completion Date:</td>
                            <td class="Level2Data">
                                <table cellpadding="0" cellspacing="0" border="0">
                                    <tr>
                                        <td></td>
                                        <td>&nbsp;&nbsp;&nbsp;</td>
                                        <td class="Level2Header">Month</td>
                                        <td>&nbsp;&nbsp;&nbsp;</td>
                                        <td class="Level2Header">Year</td>
                                        <td>&nbsp;&nbsp;&nbsp;</td>
                                        <td></td>
                                        <td>&nbsp;&nbsp;&nbsp;</td>
                                        <td class="Level2Header">Month</td>
                                        <td>&nbsp;&nbsp;&nbsp;</td>
                                        <td class="Level2Header">Year</td>
                                    </tr>
                                    <tr>
                                        <td class="Level2Header">Between</td>
                                        <td></td>
                                        <td>
                                            <asp:DropDownList ID="DropDownListFromMonth" runat="server">
                                            </asp:DropDownList></td>
                                        <td></td>
                                        <td>
                                            <asp:DropDownList ID="DropDownListFromYear" runat="server">
                                            </asp:DropDownList></td>
                                        <td></td>
                                        <td class="Level2Header">and</td>
                                        <td></td>
                                        <td>
                                            <asp:DropDownList ID="DropDownListToMonth" runat="server">
                                            </asp:DropDownList></td>
                                        <td></td>
                                        <td>
                                            <asp:DropDownList ID="DropDownListToYear" runat="server">
                                            </asp:DropDownList></td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Test:</td>
                            <td class="Level2Data">
                                <asp:DropDownList ID="DropDownListTest" runat="server" Font-Size="8pt">
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 28px">Notes: <span style="font-size:8pt; font-weight:normal">(single phrase within field)</span></td>
                            <td class="Level2Data" style="height: 28px">
                                <asp:TextBox ID="TextBoxNotes" runat="server" Width="250px"></asp:TextBox></td>
                        </tr>
                        <tr><td colspan="2" class="Level2Data">&nbsp;</td></tr>
                        <tr>
                            <td colspan="2" class="Level2Data" style="vertical-align:middle;height:40px;text-align:center">
                                &nbsp;<asp:Button ID="ButtonSearch" runat="server" Text="Go" OnClick="ButtonSearch_Click" Width="60px" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ButtonClear" runat="server" Text="Clear" Width="60px" OnClick="ButtonClear_Click" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
        </table>
        <!-- DIV needed for GetEmployees.aspx: -->
        <div id="empList" style="VISIBILITY: hidden; POSITION: absolute"><select ondblclick="SetEmployee()" style="WIDTH:250px" size="10" name="EmpMatches"></select>
        </div>        
    </form>
</body>
</html>

