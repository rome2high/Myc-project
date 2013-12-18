<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchResults.aspx.cs" Inherits="RequestTracker.SearchResults" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Request Tracker - Search Results</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
</head>
<body>
    <form id="form1" runat="server">
        <table cellpadding="0" cellspacing="0" border="0" class="Level1">
            <tr>
                <td colspan="2" class="Level1Caption">Search Results</td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
            <tr>
                <td class="Level1Data">
                    <table cellpadding="0" cellspacing="0" border="0" class="Level2" style="width:100%">
                        <tr><td class="Level2Data"><asp:Literal ID="LiteralNumRecords" runat="server"></asp:Literal> records found</td></tr>
                        <tr>
                            <td class="Level2Header" style="font-weight:normal">
                                <asp:datagrid id="DataGridJobs" runat="server" Width="100%" Font-Names="Verdana" CellPadding="1"
                                    BackColor="#DDDDFF" AutoGenerateColumns="False" Font-Size="8pt" AllowSorting="True" OnSortCommand="DataGridJobs_SortCommand">
                                    <AlternatingItemStyle Font-Size="8pt" HorizontalAlign="Left" ForeColor="Black" BackColor="White"></AlternatingItemStyle>
                                    <ItemStyle Font-Size="8pt" HorizontalAlign="Left" ForeColor="Black" BackColor="Gainsboro"></ItemStyle>
                                    <HeaderStyle Font-Size="8pt" Font-Bold="True" HorizontalAlign="Left" ForeColor="White" BackColor="#6B78A2"></HeaderStyle>
                                    <Columns>
                                        <asp:HyperLinkColumn DataNavigateUrlField="URL" DataTextField="JobID" HeaderText="Request #" SortExpression="JobID">
                                        </asp:HyperLinkColumn>
                                        <asp:BoundColumn DataField="Requestor" HeaderText="Requestor" SortExpression="Requestor"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="Quantity" HeaderText="Quantity" SortExpression="Quantity"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="EntryDate" HeaderText="Entry Date" SortExpression="EntryDate"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="CompletionDate" HeaderText="Completion Date" SortExpression="CompletionDate"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="TotalTests" HeaderText="# of Tests" SortExpression="TotalTests"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="Technician" HeaderText="Technician" SortExpression="Technician"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="FormURL" HeaderText="Form"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="Notes" HeaderText="Notes" SortExpression="Notes"></asp:BoundColumn>
                                    </Columns>
                                </asp:datagrid>
                            </td>
                        </tr>
                        <tr><td class="Level2Data">&nbsp;</td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="2" class="Level1Data">&nbsp;</td></tr>
            <tr>
                <td style="text-align:center" class="Level1Data"><input id="Button1" type="button" value="Home" onclick="window.location.pathname='/RequestTracker/home.aspx'" style="width:60px" /></td>
            </tr>
            <tr><td class="Level1Data">&nbsp;</td></tr>
        </table>
    </form>
</body>
</html>
