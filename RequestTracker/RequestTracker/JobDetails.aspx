<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JobDetails.aspx.cs" Inherits="RequestTracker.JobDetails" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Request Tracker - Request Details</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
    <!-- Javascript needed for GetEmployees.aspx: -->
    <script type="text/javascript" src="/WebTools/EmployeeLookup/GetEmployees.js"></script>
    <script type="text/javascript" src="/WebTools/ModelNumberLookup/ObjectPositioning.js"></script>
    <!-- Javascript needed for Calendar.aspx: -->
    <script type="text/javascript" src="/WebTools/Calendar/Calendar.js"></script>    
</head>
<body onclick="">
    <form id="form1" runat="server">
        <table cellpadding="0" cellspacing="0" border="0" class="Level1" style="width:70%">
            <tr>
                <td colspan="2" class="Level1Caption">Request Details</td>
            </tr>
            <tr>
                <td colspan="2" class="Level1Data">&nbsp;</td>
            </tr>
            <tr>
                <td class="Level1Data">
                    <table cellpadding="0" cellspacing="0" border="0" class="Level2" style="width:100%">
                        <tr>
                            <td class="Level2Header">Request Number:</td>
                            <td class="Level2Data" style="font-weight:bold; font-size:11pt; color:black;">
                                <asp:Literal ID="LiteralJobNumber" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Requestor:</td>
                            <td class="Level2Data">
                                <asp:TextBox ID="TextBoxEmployee" runat="server" Width="250px"></asp:TextBox>
                                 <span style="font-size:8pt; font-weight:normal">(last name, first name)</span>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 24px">Entry Date:</td>
                            <td class="Level2Data" style="font-size:11pt; color:black; height: 24px;">
                                <asp:Literal ID="LiteralEntryDate" runat="server"></asp:Literal></td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 25px">Quantity:</td>
                            <td class="Level2Data" style="height: 25px">
                                <asp:DropDownList ID="DropDownListQuantity" runat="server">
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 25px; padding-bottom: 2px;">Request Date:</td>
                            <td class="Level2Data" style="height: 25px; padding-bottom: 2px;">
                                <asp:textbox id="TextBoxStartDate3" runat="server" MaxLength="11"></asp:textbox>
                                <button onmouseover="window.status='Date Picker';return true;" style="border-style: none; border-width: 0px; HEIGHT: 25px; WIDTH: 35px; background-color: #F0FFF0;"
                                    id="Button5" onclick="ShowCalendar(this,'no')" onmouseout="window.status='';return true;"
                                    type="button"><img src="/WebTools/Calendar/Calendar.gif" alt="Date Time Picker" 
                                        border="0" style="width:20px; height:20px ; margin:0 auto" /></button>
                            </td>
                        </tr>
                        <tr style="vertical-align:top">
                            <td class="Level2Header">Tests:</td>
                            <td class="Level2Data">
                                <asp:datagrid id="DataGridTests" runat="server" Width="100%" Font-Names="Verdana" CellPadding="1"
                                    BackColor="#DDDDFF" AutoGenerateColumns="False" Font-Size="8pt" OnCancelCommand="DataGridTests_CancelCommand" OnDeleteCommand="DataGridTests_DeleteCommand" OnEditCommand="DataGridTests_EditCommand" OnItemCommand="DataGridTests_ItemCommand" OnUpdateCommand="DataGridTests_UpdateCommand">
                                    <FooterStyle Font-Size="8pt"></FooterStyle>
                                    <SelectedItemStyle Font-Size="8pt" HorizontalAlign="Center" ForeColor="Black" BackColor="PowderBlue"></SelectedItemStyle>
                                    <EditItemStyle Font-Size="8pt"></EditItemStyle>
                                    <AlternatingItemStyle Font-Size="8pt" HorizontalAlign="Left" ForeColor="Black" BackColor="White"></AlternatingItemStyle>
                                    <ItemStyle Font-Size="8pt" HorizontalAlign="Left" ForeColor="Black" BackColor="Gainsboro"></ItemStyle>
                                    <HeaderStyle Font-Size="8pt" Font-Bold="True" HorizontalAlign="Left" ForeColor="White" BackColor="#6B78A2"></HeaderStyle>
                                    <Columns>
                                        <asp:TemplateColumn>
                                            <ItemTemplate>
                                                <asp:LinkButton id="RemoveTest" CommandName="Delete" Text="Remove" Runat="server"></asp:LinkButton>
                                            </ItemTemplate>
                                            <FooterStyle Wrap="False"></FooterStyle>
                                            <FooterTemplate>
                                                <asp:LinkButton Runat="server" CommandName="Add" Text="Ok" ID="EditTestRun"></asp:LinkButton>
                                                <asp:LinkButton Runat="server" CommandName="Cancel" Text="Cancel" ID="CancelEdit"></asp:LinkButton>
                                            </FooterTemplate>
                                        </asp:TemplateColumn>
                                        <asp:EditCommandColumn ButtonType="LinkButton" UpdateText="Ok" HeaderText="Edit" CancelText="Cancel" EditText="Edit">
                                        </asp:EditCommandColumn>
                                        <asp:TemplateColumn HeaderText="Run<br/>Order">
                                            <ItemTemplate>
                                                <%# DataBinder.Eval(Container.DataItem, "RunOrder").ToString()%>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:DropDownList Font-Size="8pt" id="AddRunOrder" runat="server" DataSource='<%# GetRunOrders() %>' DataTextField="RunOrderValue" DataValueField="RunOrderID">
                                                </asp:DropDownList>
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:DropDownList Font-Size="8pt" id="EditRunOrder" runat="server" DataSource='<%# GetRunOrders() %>' SelectedIndex='<%# GetRunOrderIndex(DataBinder.Eval(Container.DataItem, "RunOrder").ToString()) %>' DataTextField="RunOrderValue" DataValueField="RunOrderID">
                                                </asp:DropDownList>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Test Name">
                                            <ItemTemplate>
                                                <%# DataBinder.Eval(Container, "DataItem.TestName") %>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:DropDownList Font-Size="8pt" id="AddTest" runat="server" DataSource='<%# GetTests() %>' DataTextField="TestName" DataValueField="TestID">
                                                </asp:DropDownList>
                                            </FooterTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Restarts">
                                            <ItemTemplate>
                                                <%#DataBinder.Eval(Container.DataItem, "Restarts").ToString()%>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:DropDownList Font-Size="8pt" id="AddRestart" runat="server" DataSource='<%# GetRestarts() %>' DataTextField="RestartValue" DataValueField="RestartID">
                                                </asp:DropDownList>
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:DropDownList Font-Size="8pt" id="EditRestart" runat="server" DataSource='<%# GetRestarts() %>' SelectedIndex='<%# GetRestartIndex(DataBinder.Eval(Container.DataItem, "Restarts").ToString()) %>' DataTextField="RestartValue" DataValueField="RestartID">
                                                </asp:DropDownList>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                    </Columns>
                                </asp:datagrid>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Data"></td>
                            <td style="height:30px;vertical-align:middle" class="Level2Data">
                                <table cellpadding="0" cellspacing="0" border="0" style="width:100%">
                                    <tr>
                                        <td style="width:26%">
                                            <asp:button id="ButtonAddTest" runat="server" Text="Add Test" 
                                                OnClick="ButtonAddTest_Click" Width="82px"></asp:button></td>
                                        <td style="width:74%;text-align:right">
                                            <asp:Literal ID="LiteralRequestInstructions" runat="server">
                                                When tests are run more than once, the list must contain a separate row for each run.&nbsp;&nbsp;&nbsp;&nbsp;<br />
                                            Restarts = Number of times the test terminated prematurely and had to be restarted.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                            </asp:Literal>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header">Technician:</td>
                            <td class="Level2Data">
                                <asp:TextBox ID="TextBoxTechnician" runat="server" Width="250px"></asp:TextBox>
                                 <span style="font-size:8pt; font-weight:normal">(last name, first name)</span>
                            </td>
                        </tr>
                        <tr style="vertical-align:top">
                            <td class="Level2Header">Notes:</td>
                            <td class="Level2Data"><asp:TextBox ID="TextBoxNotes" runat="server" Columns="50" 
                                    Rows="8" TextMode="MultiLine" Height="137px" Width="561px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 5px" colspan="2">&nbsp;</td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 25px">Start Date:</td>
                            <td class="Level2Data" style="height: 25px">
                                <asp:textbox id="TextBoxStartDate" runat="server" MaxLength="11"></asp:textbox>
                                <button onmouseover="window.status='Date Picker';return true;" style="border-style: none; border-width: 0px; HEIGHT: 25px; WIDTH: 35px; background-color: #F0FFF0;"
                                    id="Button1" onclick="ShowCalendar(this,'no')" onmouseout="window.status='';return true;"
                                    type="button"><img src="/WebTools/Calendar/Calendar.gif" alt="Date Time Picker" 
                                        border="0" style="width:20px; height:20px ; margin:0 auto" /></button>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 25px">Promise Date:</td>
                            <td class="Level2Data" style="height: 25px">
                                <asp:textbox id="TextBoxStartDate4" runat="server" MaxLength="11"></asp:textbox>
                                <button onmouseover="window.status='Date Picker';return true;" style="border-style: none; border-width: 0px; HEIGHT: 25px; WIDTH: 35px; background-color: #F0FFF0;"
                                    id="Button6" onclick="ShowCalendar(this,'no')" onmouseout="window.status='';return true;"
                                    type="button"><img src="/WebTools/Calendar/Calendar.gif" alt="Date Time Picker" 
                                        border="0" style="width:20px; height:20px ; margin:0 auto" /></button>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="height: 25px">Completion Date:</td>
                            <td class="Level2Data" style="height: 25px">
                                <asp:textbox id="TextBoxDate" runat="server" MaxLength="11"></asp:textbox>
                                <button onmouseover="window.status='Date Picker';return true;" style="border-style: inset; border-width: 0px; HEIGHT: 25px; WIDTH: 35px; background-color: #F0FFF0;"
                                    id="dtpk1" onclick="ShowCalendar(this,'no')" onmouseout="window.status='';return true;"
                                    type="button"><img src="/WebTools/Calendar/Calendar.gif" alt="Date Time Picker" 
                                        border="0" style="width:20px; height:20px ; margin:0 auto" /></button>
                            </td>
                        </tr>
                        <tr>
                            <td class="Level2Header" style="padding: 2px">
                                Form:</td>
                            <td class="Level2Data" style="padding: 3px">
                                <asp:FileUpload ID="FileUploadForm" runat="server" />
                                <span style="font-size:8pt; font-weight:normal">(.pdf format only)</span>
                                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:hyperlink id="PdfHyperLink" runat="server" Target="_blank" Font-Size="12pt" Font-Bold="true">Get Form</asp:hyperlink>
                            </td>
                        </tr>
                        <tr><td colspan="2" class="Level2Data">&nbsp;</td></tr>
                        <tr>
                            <td colspan="2" style="text-align:center;vertical-align:middle;height:40px" class="Level2Data">
                                <asp:Button ID="ButtonSave" runat="server" Text="Save" Width="60px" OnClick="ButtonSave_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ButtonDelete" runat="server" Text="Delete" Width="60px" OnClick="ButtonDelete_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ButtonBack" runat="server" Text="Back" Width="60px" OnClick="ButtonBack_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="ButtonHome" runat="server" Text="Home" Width="60px" OnClick="ButtonHome_Click" /></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;<asp:Label ID="LabelFormStatus" runat="server" CssClass="error"></asp:Label></td></tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
        </table>
        <!-- DIV needed for GetEmployees.aspx: -->
        <div id="empList" style="VISIBILITY: hidden; POSITION: absolute"><select ondblclick="SetEmployee()" style="WIDTH:250px" size="10" name="EmpMatches"></select>
        </div>        
    </form>
</body>
</html>
