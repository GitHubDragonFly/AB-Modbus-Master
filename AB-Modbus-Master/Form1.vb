' * VB Net Version by DragonFly (with embedded dlls)
' *
' * This program is a standalone Master application for Allen Bradley (ControlLogix, MicroLogix, SLC, PLC5), Omron NJNX and Modbus PLCs.
' *
' * Intended to be used solely for troubleshooting purposes.
' *
' * It is using:
' *  - libplctag C library ( https://github.com/kyle-github/libplctag ) v2.1.22
' *    licensed under Mozilla Public License Version 2.0 ( https://www.mozilla.org/en-US/MPL/2.0/ )
' *  - Modified C# Wrapper by Michele Cattafesta ( https://github.com/mesta1/libplctag-csharp )
' *    licensed under MIT license( http://opensource.org/licenses/mit-license.php ).
' *
' * This app is licensed under MPL 2.0 (the MIT license of the C# Wrapper is included in the Resources folder).
' *
' * Useful Info: https://www.mesta-automation.com/how-to-communicate-to-an-allen-bradley-plc-with-c-and-libplctag-ethernet-ip-library/
' *
' * Known Issues (as tested with MicroLogix 1100):
' * 1) Each Read/Write transaction Is limited to 236 bytes of data (for example, you can read up to 2 strings of 84 bytes each).
' * 2) SLC Timer / Counter / Control data types currently work for reading only (writing is not implemented but is possible with Int16 data type)
' *
' * Interpretation of some LGX tag type values, shown when the "Get LGX Tags" button is used:
' * - bit 15 of the value defines whether it's atomic (0) or structured (1) tag
' * - bits 14 and 13 of the value define tag array dimensions - 0 (00), 1 (01), 2 (10) or 3 (11)
' * - bit 12 of the value Is reserved And defines whether it's a system (1) tag
' * - bits 0 to 11 of the value depend on bit 15 for interpretation
' *
' * Value              Type
' * 193 (0xC1)         BOOL (1 byte)
' * 194 (0xC2)         SINT (1 byte)
' * 195 (0xC3)         INT (2 bytes)
' * 196 (0xC4)         DINT (4 bytes)
' * 197 (0xC5)         LINT (8 bytes)
' * 198 (0xC6)         USINT (1 byte)
' * 199 (0xC7)         UINT (2 bytes)
' * 200 (0xC8)         UDINT (4 bytes)
' * 201 (0xC9)         ULINT (8 bytes)
' * 202 (0xCA)         REAL (4 bytes)
' * 203 (0xCB)         LREAL (8 bytes)
' * 211 (0xD3)         BOOL ARRAY [x] (4 bytes) see also 8403
' * 8387               INT ARRAY [x] (2 bytes)
' * 8388               DINT ARRAY [x] (4 bytes)
' * 8394               REAL ARRAY [x] (4 bytes)
' * 8403               BOOL ARRAY [x] (4 bytes) see also 211
' * 16579              INT ARRAY [x, y] (2 bytes)
' * 16580              DINT ARRAY [x, y] (4 bytes)
' * 16586              REAL ARRAY [x, y] (4 bytes)
' * 24772              DINT ARRAY [x, y, z] (4 bytes)
' * 24778              REAL ARRAY [x, y, z] (4 bytes)
' * 34920              CUSTOM LENGTH STRING (8 bytes) - 1char
' * 36171              CUSTOM LENGTH STRING (24 bytes) - 20char
' * 35541              TIME (28 bytes)
' * 36737              CONTROL (12 bytes)
' * 36738              COUNTER (12 bytes)
' * 36739              TIMER (12 bytes)
' * 36814              STRING (88 bytes)
' * 45006              STRING ARRAY [x] (88 bytes)
' *
' * For arrays, the bytes shown are for each element.
' *
' * See a few more in this conversation: https://github.com/libplctag/libplctag4android/issues/1

Imports System.Numerics

Public Class Form1

    Public libplctagIsNew, pollInterval, cpuTypeIndex As Integer

    Private Master As New LibplctagWrapper.Libplctag
    Private AutoReadMaster As New LibplctagWrapper.Libplctag
    Private tag1, tag2in As LibplctagWrapper.Tag
    Private elementSize, elementCount As Integer
    Private RealElementCount1, RealElementCount2 As Integer
    Private StrMessage As String = ""
    Private PIDSuffix As String = ""
    Private AutoReadStrMessage As String = ""
    Private AutoReadPIDSuffix As String = ""
    Private ReadOnly libraryVersion As String = ""
    Private ReadOnly AllToolTip As New ToolTip
    Private AutoReadBckgndThread As Threading.Thread
    Private ReadOnly m_Lock As New Object
    Private CustomStringLength, AutoReadCustomStringLength As Integer

    Private Tag1ProcessingTask As Threading.Tasks.Task

    Private Structure PollInfo
        Friend Index As Integer
        Friend PlcAddress As String
        Friend DataType As String
        Friend bitIndex As Integer
        Friend AutoReadElementCount As Integer
        Friend AutoReadElementSize As Integer
        Friend cpuType As LibplctagWrapper.CpuType
        Friend tag2 As LibplctagWrapper.Tag
        Friend ARPIDSuffix As String
    End Structure

    Private Shared ReadOnly AutoPollAddressList As New List(Of PollInfo)

    Private Structure AddressInfo
        Friend RadioBtn As RadioButton
        Friend PlcAddress As TextBox
        Friend CheckBoxRead As CheckBox
        Friend CheckBoxWrite As CheckBox
        Friend ValuesToWrite As TextBox
        Friend ButtonSend As Button
        Friend LabelStatus As Label
        Friend ModbusAddress As Label
    End Structure

    Private Shared ReadOnly AddressList(11) As AddressInfo

    Private byteOrder As String = Nothing
    Private AutoReadbyteOrder As String = Nothing
    Private ReadOnly int16byteOrder As String() = New String() {"int16_byte_order=10", "int16_byte_order=01"}
    Private ReadOnly int32byteOrder As String() = New String() {"int32_byte_order=3210", "int32_byte_order=2301", "int32_byte_order=1032", "int32_byte_order=0123"}
    Private ReadOnly int64byteOrder As String() = New String() {"int64_byte_order=76543210", "int64_byte_order=67452301", "int64_byte_order=10325476", "int64_byte_order=01234567"}
    Private ReadOnly float32byteOrder As String() = New String() {"float32_byte_order=3210", "float32_byte_order=2301", "float32_byte_order=1032", "float32_byte_order=0123"}
    Private ReadOnly float64byteOrder As String() = New String() {"float64_byte_order=76543210", "float64_byte_order=67452301", "float64_byte_order=10325476", "float64_byte_order=01234567"}

#Region "Constructor"

    Public Sub New()
        InitializeComponent()
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.ContainerControl Or ControlStyles.SupportsTransparentBackColor, True)

        libplctagIsNew = Master.CheckPlctagLibraryVersion(2, 2, 0)

        libraryVersion = "libplctag v" & Master.GetLibraryIntAttribute(0, "version_major", 0) & "." & Master.GetLibraryIntAttribute(0, "version_minor", 0) & "." & Master.GetLibraryIntAttribute(0, "version_patch", 0)
        lblLibVersion.Text = libraryVersion
    End Sub

#End Region

#Region "Private Methods"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim j As Integer = 0

        While j < 12
            AddressList(j) = New AddressInfo With {.ModbusAddress = New Label With {.Text = ""}}

            For Each ctrl As Control In Controls
                For Each Child As Control In ctrl.Controls
                    If TypeOf Child Is RadioButton Then
                        If Child.Name.Equals("rb" & j) Then AddressList(j).RadioBtn = Child
                    ElseIf TypeOf Child Is TextBox Then
                        If Child.Name.Equals("tbAddr" & j) Then
                            AddressList(j).PlcAddress = Child
                        ElseIf Child.Name.Equals("tbVtoW" & j) Then
                            AddressList(j).ValuesToWrite = Child
                        End If
                    ElseIf TypeOf Child Is CheckBox Then
                        If Child.Name.Equals("cbRead" & j) Then
                            AddressList(j).CheckBoxRead = Child
                        ElseIf Child.Name.Equals("cbWrite" & j) Then
                            AddressList(j).CheckBoxWrite = Child
                        End If
                    ElseIf TypeOf Child Is Button Then
                        If Child.Name.Equals("btnSend" & j) Then AddressList(j).ButtonSend = Child
                    ElseIf TypeOf Child Is Label Then
                        If Child.Name.Equals("lblStatus" & j) Then AddressList(j).LabelStatus = Child
                    End If
                Next
            Next

            j += 1
        End While

        cbCPUType.SelectedIndex = 3
        cbProtocol.SelectedIndex = 0
        cbPollInterval.SelectedIndex = 4

        Focus()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If chbAutoRead.Checked Then chbAutoRead.Checked = False

        Dim index As Integer
        While index < My.Application.OpenForms.Count 'If Form2 is open and hidden then close it
            If My.Application.OpenForms(index) IsNot Me Then
                My.Application.OpenForms(index).Close()
            End If
            index += 1
        End While

        If AutoReadMaster IsNot Nothing Then
            AutoReadMaster.Dispose()
            AutoReadMaster = Nothing
        End If

        If Master IsNot Nothing Then
            Master.Dispose()
            Master = Nothing
        End If

        Dispose(True)
    End Sub

    Private Sub ComboBoxCPUType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCPUType.SelectedIndexChanged
        Master = New LibplctagWrapper.Libplctag()
        AutoReadMaster = New LibplctagWrapper.Libplctag()

        Dim j As Integer = 0

        While j < 12 'Clear all addresses
            If Not String.IsNullOrWhiteSpace(AddressList(j).PlcAddress.Text) Then AddressList(j).PlcAddress.Text = ""
            j += 1
        End While

        If cbCPUType.SelectedIndex = 7 Then 'MODBUS
            If cbProtocol.SelectedIndex <> 1 Then cbProtocol.SelectedIndex = 1
            tbIPAddress.Text = "127.0.0.1"
            lblPath.Text = "UnitID"
            tbPath.Text = "1"
            gbModbus.Visible = True
            tbProgramName.Enabled = False
            btnGetTags.Enabled = False
            cbTags.Items.Clear()
            cbTags.Enabled = False
        Else
            If cbProtocol.SelectedIndex <> 0 Then cbProtocol.SelectedIndex = 0
            lblPath.Text = "Path"
            gbModbus.Visible = False

            If cbCPUType.SelectedIndex = 0 Then 'ControlLogix
                tbIPAddress.Text = "192.168.1.24"
                tbProgramName.Enabled = True
                tbPath.Text = "1,3"
                btnGetTags.Enabled = True
                cbTags.Items.Clear()
                cbTags.Enabled = True
            ElseIf cbCPUType.SelectedIndex = 2 OrElse cbCPUType.SelectedIndex = 6 Then 'Logixpccc/NJNX
                tbIPAddress.Text = "192.168.1.10"
                tbProgramName.Enabled = True
                tbPath.Text = "1,0"
                btnGetTags.Enabled = False
                cbTags.Items.Clear()
                cbTags.Enabled = False
            Else
                tbIPAddress.Text = "192.168.1.10"
                tbProgramName.Enabled = False
                tbPath.Text = ""
                btnGetTags.Enabled = False
                cbTags.Items.Clear()
                cbTags.Enabled = False
            End If
        End If

        cpuTypeIndex = cbCPUType.SelectedIndex
    End Sub

    Private Sub ComboBoxProtocol_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbProtocol.SelectedIndexChanged
        If cbProtocol.SelectedIndex = 0 Then
            If cbCPUType.SelectedIndex = 7 Then
                cbCPUType.SelectedIndex = 3
                lblPath.Text = "Path"
            End If
        Else
            If cbCPUType.SelectedIndex <> 7 Then
                cbCPUType.SelectedIndex = 7
                lblPath.Text = "UnitID"
            End If
        End If
    End Sub

    Private Sub ComboBoxTags_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbTags.SelectedIndexChanged
        If Not (String.IsNullOrWhiteSpace(cbTags.SelectedItem.ToString) OrElse cbTags.SelectedItem.ToString.StartsWith("***")) Then
            Clipboard.SetText(cbTags.SelectedItem.ToString)
        End If
    End Sub

    Private Sub ComboBoxPollInterval_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbPollInterval.SelectedIndexChanged
        pollInterval = cbPollInterval.SelectedItem.ToString
    End Sub

    Private Sub CheckBoxBDOneZero_CheckedChanged(sender As Object, e As EventArgs) Handles chbBDOneZero.CheckedChanged
        If chbBDOneZero.Checked Then
            If chbBDOnOff.Checked Then chbBDOnOff.Checked = False
        End If
    End Sub

    Private Sub CheckBoxBDOnOff_CheckedChanged(sender As Object, e As EventArgs) Handles chbBDOnOff.CheckedChanged
        If chbBDOnOff.Checked Then
            If chbBDOneZero.Checked Then chbBDOneZero.Checked = False
        End If
    End Sub

    Private Sub TextBoxPath_TextChanged(sender As Object, e As EventArgs) Handles tbPath.TextChanged
        If cbCPUType.SelectedIndex = 7 Then
            Dim dummy As Integer
            If Not Integer.TryParse(tbPath.Text, dummy) Then
                MessageBox.Show("Modbus UnitID has to be integer value (1 to 247)!")
                tbPath.Text = "1"
            Else
                If dummy < 1 OrElse dummy > 247 Then
                    MessageBox.Show("Modbus UnitID has to be integer value (1 to 247)!")
                    tbPath.Text = "1"
                End If
            End If
        End If
    End Sub

    Private Sub TextBoxTimeout_TextChanged(sender As Object, e As EventArgs) Handles tbTimeout.TextChanged
        Dim dummy As Integer
        If Integer.TryParse(tbTimeout.Text, dummy) Then
            If dummy < 1 Then
                MessageBox.Show("Positive non-zero integer value required for timeout!")
                tbTimeout.Text = "5000"
            End If
        Else
            MessageBox.Show("Positive non-zero integer value required for timeout!")
            tbTimeout.Text = "5000"
        End If
    End Sub

#End Region

#Region "Read / Write Methods"

    ' Radio buttons to clear the address
    Private Sub RadioButtonReset_Click(sender As Object, e As EventArgs) Handles rb0.Click, rb1.Click, rb2.Click, rb3.Click, rb4.Click, rb5.Click, rb6.Click, rb7.Click, rb8.Click, rb9.Click, rb10.Click, rb11.Click
        Dim sndr As RadioButton = DirectCast(sender, RadioButton)
        Dim sndrIndex As Integer = sndr.Name.Substring(2)

        If Not String.IsNullOrWhiteSpace(AddressList(sndrIndex).PlcAddress.Text) Then AddressList(sndrIndex).PlcAddress.Text = ""
    End Sub

    Private Sub TextBoxAddress_KeyDown(sender As Object, e As KeyEventArgs) Handles tbAddr9.KeyDown, tbAddr8.KeyDown, tbAddr7.KeyDown, tbAddr6.KeyDown, tbAddr5.KeyDown, tbAddr4.KeyDown, tbAddr3.KeyDown, tbAddr2.KeyDown, tbAddr11.KeyDown, tbAddr10.KeyDown, tbAddr1.KeyDown, tbAddr0.KeyDown
        Dim sndrIndex As Integer = sender.Name.Substring(6)

        If cbCPUType.SelectedIndex = 7 Then
            Form2.modbus = True
            If AddressList(sndrIndex).ModbusAddress.Text = "" Then
                Form2.Txt = ""
                Form2.ElementCount = "1"
            Else
                Form2.Txt = AddressList(sndrIndex).ModbusAddress.Text.Substring(0, AddressList(sndrIndex).ModbusAddress.Text.IndexOf(","))
                Form2.ElementCount = AddressList(sndrIndex).ModbusAddress.Text.Substring(AddressList(sndrIndex).ModbusAddress.Text.IndexOf(",") + 1)
            End If
        Else
            Form2.modbus = False
            Form2.Txt = DirectCast(sender, TextBox).Text
        End If

        Dim dg As DialogResult = Form2.ShowDialog()
        If dg = Windows.Forms.DialogResult.OK Then
            If cbCPUType.SelectedIndex = 7 Then
                sender.Text = Form2.tbMFResultingAddress.Text.Trim
                AddressList(sndrIndex).ModbusAddress.Text = Form2.tbMResultingAddress.Text.Trim & "," & Form2.cbEC.SelectedItem.ToString
            Else
                sender.Text = Form2.tbABResultingAddress.Text.Trim
            End If
        End If
    End Sub

    Private Sub TextBoxAddress_MouseClick(sender As Object, e As MouseEventArgs) Handles tbAddr0.MouseClick, tbAddr1.MouseClick, tbAddr2.MouseClick, tbAddr3.MouseClick, tbAddr4.MouseClick, tbAddr5.MouseClick, tbAddr6.MouseClick, tbAddr7.MouseClick, tbAddr8.MouseClick, tbAddr9.MouseClick, tbAddr10.MouseClick, tbAddr11.MouseClick
        Dim sndrIndex As Integer = sender.Name.Substring(6)

        If cbCPUType.SelectedIndex = 7 Then
            Form2.modbus = True
            If AddressList(sndrIndex).ModbusAddress.Text = "" Then
                Form2.Txt = ""
                Form2.ElementCount = "1"
            Else
                Form2.Txt = AddressList(sndrIndex).ModbusAddress.Text.Substring(0, AddressList(sndrIndex).ModbusAddress.Text.IndexOf(","))
                Form2.ElementCount = AddressList(sndrIndex).ModbusAddress.Text.Substring(AddressList(sndrIndex).ModbusAddress.Text.IndexOf(",") + 1)
            End If
        Else
            Form2.modbus = False
            Form2.Txt = DirectCast(sender, TextBox).Text
        End If

        Dim dg As DialogResult = Form2.ShowDialog()
        If dg = Windows.Forms.DialogResult.OK Then
            If cbCPUType.SelectedIndex = 7 Then
                sender.Text = Form2.tbMFResultingAddress.Text.Trim
                AddressList(sndrIndex).ModbusAddress.Text = Form2.tbMResultingAddress.Text.Trim & "," & Form2.cbEC.SelectedItem.ToString
            Else
                sender.Text = Form2.tbABResultingAddress.Text.Trim
            End If
        End If
    End Sub

    Private Sub TextBoxAddress_TextChanged(sender As Object, e As EventArgs) Handles tbAddr0.TextChanged, tbAddr1.TextChanged, tbAddr2.TextChanged, tbAddr3.TextChanged, tbAddr4.TextChanged, tbAddr5.TextChanged, tbAddr6.TextChanged, tbAddr7.TextChanged, tbAddr8.TextChanged, tbAddr9.TextChanged, tbAddr10.TextChanged, tbAddr11.TextChanged
        Dim sndr As TextBox = DirectCast(sender, TextBox)
        Dim sndrIndex As Integer = sndr.Name.Substring(6)

        ' Enable/Disable corresponding controls
        If Not String.IsNullOrWhiteSpace(sndr.Text) Then
            AddressList(sndrIndex).CheckBoxRead.Enabled = True
            AddressList(sndrIndex).CheckBoxRead.Checked = True
            AddressList(sndrIndex).CheckBoxWrite.Enabled = True
            AddressList(sndrIndex).CheckBoxWrite.Checked = False
        Else
            AddressList(sndrIndex).CheckBoxRead.Checked = False
            AddressList(sndrIndex).CheckBoxRead.Enabled = False
            AddressList(sndrIndex).CheckBoxWrite.Checked = False
            AddressList(sndrIndex).CheckBoxWrite.Enabled = False
            AddressList(sndrIndex).ValuesToWrite.Text = ""
            AddressList(sndrIndex).ValuesToWrite.ReadOnly = True
            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.White
            AddressList(sndrIndex).ButtonSend.BackColor = Color.Gainsboro
            AddressList(sndrIndex).ButtonSend.Enabled = False
            AddressList(sndrIndex).LabelStatus.BackColor = Color.White
        End If
    End Sub

    Private Sub CheckBoxRead_EnabledChanged(sender As Object, e As EventArgs) Handles cbRead0.EnabledChanged, cbRead1.EnabledChanged, cbRead2.EnabledChanged, cbRead3.EnabledChanged, cbRead4.EnabledChanged, cbRead5.EnabledChanged, cbRead6.EnabledChanged, cbRead7.EnabledChanged, cbRead8.EnabledChanged, cbRead9.EnabledChanged, cbRead10.EnabledChanged, cbRead11.EnabledChanged
        If sender.Enabled Then
            sender.Visible = True
        Else
            sender.Visible = False
        End If
    End Sub

    Private Sub CheckBoxRead_Click(sender As Object, e As EventArgs) Handles cbRead0.Click, cbRead1.Click, cbRead2.Click, cbRead3.Click, cbRead4.Click, cbRead5.Click, cbRead6.Click, cbRead7.Click, cbRead8.Click, cbRead9.Click, cbRead10.Click, cbRead11.Click
        Dim sndr As CheckBox = DirectCast(sender, CheckBox)

        If Not sndr.Checked Then sndr.Checked = True
    End Sub

    Private Sub CheckBoxRead_CheckedChanged(sender As Object, e As EventArgs) Handles cbRead0.CheckedChanged, cbRead1.CheckedChanged, cbRead2.CheckedChanged, cbRead3.CheckedChanged, cbRead4.CheckedChanged, cbRead5.CheckedChanged, cbRead6.CheckedChanged, cbRead7.CheckedChanged, cbRead8.CheckedChanged, cbRead9.CheckedChanged, cbRead10.CheckedChanged, cbRead11.CheckedChanged
        Dim sndr As CheckBox = DirectCast(sender, CheckBox)
        Dim sndrIndex As Integer = sndr.Name.Substring(6)

        If sndr.Checked Then
            AddressList(sndrIndex).CheckBoxWrite.Checked = False
            AddressList(sndrIndex).ValuesToWrite.Text = ""
            AddressList(sndrIndex).ValuesToWrite.ReadOnly = True
            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.White
            AddressList(sndrIndex).ButtonSend.Enabled = True
            AddressList(sndrIndex).ButtonSend.BackColor = Color.LightSteelBlue
            AddressList(sndrIndex).LabelStatus.BackColor = Color.White
        End If
    End Sub

    Private Sub CheckBoxWrite_EnabledChanged(sender As Object, e As EventArgs) Handles cbWrite0.EnabledChanged, cbWrite1.EnabledChanged, cbWrite2.EnabledChanged, cbWrite3.EnabledChanged, cbWrite4.EnabledChanged, cbWrite5.EnabledChanged, cbWrite6.EnabledChanged, cbWrite7.EnabledChanged, cbWrite8.EnabledChanged, cbWrite9.EnabledChanged, cbWrite10.EnabledChanged, cbWrite11.EnabledChanged
        If sender.Enabled Then
            sender.Visible = True
        Else
            sender.Visible = False
        End If
    End Sub

    Private Sub CheckBoxWrite_Click(sender As Object, e As EventArgs) Handles cbWrite0.Click, cbWrite1.Click, cbWrite2.Click, cbWrite3.Click, cbWrite4.Click, cbWrite5.Click, cbWrite6.Click, cbWrite7.Click, cbWrite8.Click, cbWrite9.Click, cbWrite10.Click, cbWrite11.Click
        Dim sndr As CheckBox = DirectCast(sender, CheckBox)

        If Not sndr.Checked Then sndr.Checked = True
    End Sub

    Private Sub CheckBoxWrite_CheckedChanged(sender As Object, e As EventArgs) Handles cbWrite0.CheckedChanged, cbWrite1.CheckedChanged, cbWrite2.CheckedChanged, cbWrite3.CheckedChanged, cbWrite4.CheckedChanged, cbWrite5.CheckedChanged, cbWrite6.CheckedChanged, cbWrite7.CheckedChanged, cbWrite8.CheckedChanged, cbWrite9.CheckedChanged, cbWrite10.CheckedChanged, cbWrite11.CheckedChanged
        Dim sndr As CheckBox = DirectCast(sender, CheckBox)
        Dim sndrIndex As Integer = sndr.Name.Substring(7)

        If sndr.Checked Then
            AddressList(sndrIndex).CheckBoxRead.Checked = False
            AddressList(sndrIndex).ValuesToWrite.Text = ""
            AddressList(sndrIndex).ValuesToWrite.ReadOnly = False
            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.White
            AddressList(sndrIndex).ButtonSend.Enabled = True
            AddressList(sndrIndex).ButtonSend.BackColor = Color.LightSteelBlue
            AddressList(sndrIndex).LabelStatus.BackColor = Color.White
        End If
    End Sub

    Private Sub TextBoxVtoW_DoubleClick(sender As Object, e As EventArgs) Handles tbVtoW0.DoubleClick, tbVtoW1.DoubleClick, tbVtoW2.DoubleClick, tbVtoW3.DoubleClick, tbVtoW4.DoubleClick, tbVtoW5.DoubleClick, tbVtoW6.DoubleClick, tbVtoW7.DoubleClick, tbVtoW8.DoubleClick, tbVtoW9.DoubleClick, tbVtoW10.DoubleClick, tbVtoW11.DoubleClick
        Dim sndr As TextBox = DirectCast(sender, TextBox)

        sndr.SelectAll() ' Select current text
    End Sub

    Private Sub ButtonSend_Click(sender As Object, e As EventArgs) Handles btnSend0.Click, btnSend1.Click, btnSend2.Click, btnSend3.Click, btnSend4.Click, btnSend5.Click, btnSend6.Click, btnSend7.Click, btnSend8.Click, btnSend9.Click, btnSend10.Click, btnSend11.Click
        Dim sndr As Button = DirectCast(sender, Button)
        Dim sndrIndex As Integer = sndr.Name.Substring(7)

        If Tag1ProcessingTask Is Nothing OrElse (Tag1ProcessingTask.Status <> 0 And Tag1ProcessingTask.Status <> 2 And Tag1ProcessingTask.Status <> 3) Then
            ' Set the corresponding status label back color to Yellow = Processing
            AddressList(sndrIndex).LabelStatus.BackColor = Color.Yellow

            ' Set the message label text to "Processing Request"
            lblMessage.Text = "Processing Request"

            Dim prot As String = cbProtocol.SelectedItem.ToString
            Dim ipAddress As String = tbIPAddress.Text

            'CPU Types:
            'LibplctagWrapper.CpuType.ControlLogix = 0
            'LibplctagWrapper.CpuType.Logixpccc = 1
            'LibplctagWrapper.CpuType.Micro800 = 2
            'LibplctagWrapper.CpuType.MicroLogix = 3
            'LibplctagWrapper.CpuType.Slc500 = 4
            'LibplctagWrapper.CpuType.Plc5 = 5
            'LibplctagWrapper.CpuType.NJNX = 6
            'LibplctagWrapper.CpuType.MODBUS = 7

            Dim cpuType As LibplctagWrapper.CpuType
            For Each typ As LibplctagWrapper.CpuType In [Enum].GetValues(GetType(LibplctagWrapper.CpuType))
                If typ.ToString = cbCPUType.SelectedItem Then
                    cpuType = typ
                    Exit For
                End If
            Next

            ' Split tag string based on semicolon delimiter (split parts = PLC Address, Data Type, Element Count)
            Dim stringValues = AddressList(sndrIndex).PlcAddress.Text.Split(New Char() {";"c})

            For j = 0 To stringValues.Length - 1
                stringValues(j) = stringValues(j).Replace(" ", "") 'Remove all spaces
            Next

            Dim address2poll = stringValues(0)
            Dim dataType = stringValues(1)
            elementCount = CInt(stringValues(2)) 'If Bit/Character/Array operation = number of consecutive bits/characters/array elements
            Dim bitIndex As Integer = -1

            Select Case dataType
                Case "Int8", "SINT", "UInt8", "USINT", "BOOL"
                    elementSize = 1

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If chbSwapBytes.Checked Then
                            byteOrder = int16byteOrder(0)
                        Else
                            byteOrder = int16byteOrder(1)
                        End If

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 1
                        Else
                            RealElementCount1 = elementCount
                        End If
                    End If
                Case "Int16", "INT", "UInt16", "UINT"
                    elementSize = 2

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        If chbSwapBytes.Checked Then
                            byteOrder = int16byteOrder(0)
                        Else
                            byteOrder = int16byteOrder(1)
                        End If

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 1
                        Else
                            RealElementCount1 = elementCount
                        End If
                    ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                        cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                        cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                        byteOrder = int16byteOrder(1)
                    End If
                Case "Int32", "DINT", "UInt32", "UDINT", "BOOLArray"
                    elementSize = 4

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 2
                        Else
                            RealElementCount1 = elementCount * 2
                        End If

                        If chbSwapBytes.Checked Then
                            If chbSwapWords.Checked Then
                                byteOrder = int32byteOrder(0)
                            Else
                                byteOrder = int32byteOrder(2)
                            End If
                        Else
                            If chbSwapWords.Checked Then
                                byteOrder = int32byteOrder(1)
                            Else
                                byteOrder = int32byteOrder(3)
                            End If
                        End If
                    ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                        cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                        cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                        byteOrder = int32byteOrder(3)
                    Else
                        If dataType = "BOOLArray" Then dataType = "BOOL Array"
                    End If
                Case "Float32", "REAL"
                    elementSize = 4

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 2
                        Else
                            RealElementCount1 = elementCount * 2
                        End If

                        If chbSwapBytes.Checked Then
                            If chbSwapWords.Checked Then
                                byteOrder = float32byteOrder(0)
                            Else
                                byteOrder = float32byteOrder(2)
                            End If
                        Else
                            If chbSwapWords.Checked Then
                                byteOrder = float32byteOrder(1)
                            Else
                                byteOrder = float32byteOrder(3)
                            End If
                        End If
                    ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                        cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                        byteOrder = float32byteOrder(3)
                    ElseIf (libplctagIsNew = 0) AndAlso cpuType = LibplctagWrapper.CpuType.Plc5 Then

                        byteOrder = float32byteOrder(1)
                    End If
                Case "Int64", "LINT", "UInt64", "ULINT"
                    elementSize = 8

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 4
                        Else
                            RealElementCount1 = elementCount * 4
                        End If

                        If chbSwapBytes.Checked Then
                            If chbSwapWords.Checked Then
                                byteOrder = int64byteOrder(0)
                            Else
                                byteOrder = int64byteOrder(2)
                            End If
                        Else
                            If chbSwapWords.Checked Then
                                byteOrder = int64byteOrder(1)
                            Else
                                byteOrder = int64byteOrder(3)
                            End If
                        End If
                    ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                        cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                        cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                        byteOrder = int64byteOrder(3)
                    End If
                Case "Float64", "LREAL"
                    elementSize = 8

                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 4
                        Else
                            RealElementCount1 = elementCount * 4
                        End If

                        If chbSwapBytes.Checked Then
                            If chbSwapWords.Checked Then
                                byteOrder = float64byteOrder(0)
                            Else
                                byteOrder = float64byteOrder(2)
                            End If
                        Else
                            If chbSwapWords.Checked Then
                                byteOrder = float64byteOrder(1)
                            Else
                                byteOrder = float64byteOrder(3)
                            End If
                        End If
                    ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                        cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                        cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                        byteOrder = float64byteOrder(3)
                    End If
                Case "Int128", "QDINT", "UInt128", "QUDINT"
                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        elementSize = 2

                        If address2poll.IndexOfAny("/") <> -1 Then
                            RealElementCount1 = 8
                        Else
                            RealElementCount1 = elementCount * 8
                        End If
                    Else
                        elementSize = 16
                    End If
                Case "PID" 'Only applicable to MicroLogix
                    elementSize = 2
                    RealElementCount1 = 23

                    If address2poll.IndexOfAny(".") <> -1 Then
                        PIDSuffix = stringValues(0).Substring(stringValues(0).IndexOf(".") + 1)
                        address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(".")) 'Workaround
                    End If
                Case "CustomString" 'Only applicable to ControlLogix
                    dataType = "Custom String"
                    CustomStringLength = stringValues(3)
                    elementSize = Math.Ceiling(CustomStringLength / 8.0F) * 8
                Case "String"
                    If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                        CustomStringLength = stringValues(3)
                        elementSize = 2
                        RealElementCount1 = Math.Ceiling(CustomStringLength / 2.0F)
                    ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                        elementSize = 256
                    ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                        elementSize = 88
                    Else
                        elementSize = 84
                    End If
                Case Else 'Timer or Counter or Control
                    If cpuType = 0 OrElse cpuType = 2 Then
                        If stringValues(0).EndsWith(".PRE") OrElse stringValues(0).EndsWith(".ACC") OrElse
                            stringValues(0).EndsWith(".LEN") OrElse stringValues(0).EndsWith(".POS") Then

                            elementSize = 4
                            dataType = "DINT"
                        ElseIf stringValues(0).EndsWith(".EN") OrElse stringValues(0).EndsWith(".TT") OrElse
                            stringValues(0).EndsWith(".DN") OrElse stringValues(0).EndsWith(".CU") OrElse
                            stringValues(0).EndsWith(".CD") OrElse stringValues(0).EndsWith(".OV") OrElse
                            stringValues(0).EndsWith(".UN") OrElse stringValues(0).EndsWith(".UA") OrElse
                            stringValues(0).EndsWith(".EU") OrElse stringValues(0).EndsWith(".EM") OrElse
                            stringValues(0).EndsWith(".ER") OrElse stringValues(0).EndsWith(".UL") OrElse
                            stringValues(0).EndsWith(".IN") OrElse stringValues(0).EndsWith(".FD") Then

                            elementSize = 1
                            dataType = "BOOL"
                        Else
                            elementSize = 12
                        End If
                    Else
                        If stringValues(0).EndsWith(".PRE") OrElse stringValues(0).EndsWith(".ACC") OrElse
                            stringValues(0).EndsWith(".LEN") OrElse stringValues(0).EndsWith(".POS") Then

                            elementSize = 2
                            dataType = "INT"
                        Else
                            elementSize = 6
                        End If
                    End If
            End Select

            Try
                If stringValues(0).IndexOfAny("/") <> -1 Then
                    If stringValues(0).Length > stringValues(0).IndexOf("/") + 1 Then
                        bitIndex = Convert.ToInt32(stringValues(0).Substring(stringValues(0).IndexOf("/") + 1))
                    Else
                        MessageBox.Show("No index specified!")
                        Exit Sub
                    End If

                    If dataType = "String" Then
                        If bitIndex < 1 OrElse (cpuType = 2 AndAlso bitIndex + elementCount > 255) OrElse
                            (cpuType = 0 AndAlso bitIndex + elementCount > 84) OrElse
                            (cpuType = 7 AndAlso bitIndex + elementCount > CustomStringLength) OrElse
                            (cpuType <> 2 AndAlso cpuType <> 0 AndAlso cpuType <> 7 AndAlso bitIndex + elementCount > 82) Then

                            AddressList(sndrIndex).ValuesToWrite.Text = "Error"
                            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon
                            lblMessage.Text = "Error - Out of range!"
                            Exit Sub
                        End If
                    ElseIf dataType = "Custom String" Then
                        If bitIndex < 1 OrElse bitIndex + elementCount > CustomStringLength Then
                            AddressList(sndrIndex).ValuesToWrite.Text = "Error"
                            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon
                            lblMessage.Text = "Error - Out of range!"
                            Exit Sub
                        End If
                    Else
                        If bitIndex < 0 OrElse (cpuType <> 7 AndAlso bitIndex + elementCount > elementSize * 8) OrElse (cpuType = 7 AndAlso bitIndex + elementCount > elementSize * RealElementCount1 * 8) Then
                            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon
                            AddressList(sndrIndex).ValuesToWrite.Text = "Error"
                            lblMessage.Text = "Error - Out of range!"
                            Exit Sub
                        End If
                    End If

                    address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf("/")) 'Workaround
                    If cpuType < 7 Then RealElementCount1 = 1

                ElseIf dataType = "BOOL Array" Then
                    bitIndex = CInt(stringValues(0).Substring(stringValues(0).IndexOf("[") + 1, stringValues(0).IndexOf("]") - stringValues(0).IndexOf("[") - 1))

                    If bitIndex < 0 Then
                        AddressList(sndrIndex).ValuesToWrite.Text = "Error"
                        AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon
                        lblMessage.Text = "Error - Out of range!"
                        Exit Sub
                    End If

                    RealElementCount1 = Math.Ceiling((bitIndex + elementCount) / (elementSize * 8))
                    address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf("[")) & "[0]" 'Workaround

                ElseIf stringValues(0).IndexOfAny("[") <> -1 AndAlso stringValues(0).IndexOfAny(",") <> -1 AndAlso stringValues(0).IndexOfAny("]") <> -1 Then
                    If stringValues(0).IndexOf(",") < stringValues(0).LastIndexOf(",") Then
                        If System.Text.RegularExpressions.Regex.Matches(stringValues(0), ",").Count > 2 Then
                            AddressList(sndrIndex).ValuesToWrite.Text = "Unsupported array"
                            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon
                            lblMessage.Text = "Error - Supported arrays: [x] or [x,y] or [x,y,z]!"
                            Exit Sub
                        Else
                            'Array [x, y, z]
                            address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(",")) & "][" & stringValues(0).Substring(stringValues(0).IndexOf(",") + 1, stringValues(0).LastIndexOf(",") - stringValues(0).IndexOf(",") - 1) & "][" & stringValues(0).Substring(stringValues(0).LastIndexOf(",") + 1) 'Workaround
                        End If
                    Else
                        'Array [x, y]
                        address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(",")) & "][" & stringValues(0).Substring(stringValues(0).IndexOf(",") + 1) 'Workaround
                    End If

                ElseIf cpuType <> 0 AndAlso cpuType <> 2 AndAlso cpuType <> 6 AndAlso cpuType <> 7 AndAlso (dataType = "Timer" OrElse dataType = "Counter" OrElse dataType = "Control") AndAlso stringValues(0).IndexOfAny(".") <> -1 Then
                    address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(".")) 'Workaround
                End If
            Catch ex As Exception
                MessageBox.Show(ex.Message)
                lblMessage.Text = "Error - " & ex.Message
                Exit Sub
            End Try

            Dim path As String

            If cpuType = 0 OrElse cpuType = 1 OrElse cpuType = 6 OrElse cpuType = 7 Then
                If String.IsNullOrWhiteSpace(tbPath.Text) Then
                    If cpuType = 7 Then
                        MessageBox.Show("UnitID not specified!")
                        lblMessage.Text = "Error - UnitID not specified!"
                        Exit Sub
                    Else
                        MessageBox.Show("Path not specified!")
                        lblMessage.Text = "Error - Path not specified!"
                        Exit Sub
                    End If
                End If

                path = tbPath.Text.Replace(" ", "")

                If cpuType = 7 Then
                    Dim dummy As Integer
                    If Integer.TryParse(path, dummy) Then
                        If dummy < 1 OrElse dummy > 247 Then
                            MessageBox.Show("Invalid UnitID!")
                            lblMessage.Text = "Error - Valid UnitID is 1 to 247!"
                            Exit Sub
                        End If
                    End If
                End If

                If dataType = "BOOL Array" OrElse stringValues(0).IndexOfAny("/") <> -1 OrElse cpuType = 7 Then
                    If byteOrder IsNot Nothing Then
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, elementSize, RealElementCount1, byteOrder)
                        byteOrder = Nothing
                    Else
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, elementSize, RealElementCount1)
                    End If
                Else
                    tag1 = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, elementSize, elementCount)
                End If
            Else
                If stringValues(0).IndexOfAny("/") <> -1 OrElse dataType = "PID" Then
                    If (libplctagIsNew = 0) AndAlso byteOrder IsNot Nothing Then
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, elementSize, RealElementCount1, byteOrder)
                    Else
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, elementSize, RealElementCount1)
                    End If
                Else
                    If (libplctagIsNew = 0) AndAlso byteOrder IsNot Nothing Then
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, elementSize, elementCount, byteOrder)
                    Else
                        tag1 = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, elementSize, elementCount)
                    End If
                End If
            End If

            Tag1ProcessingTask = Threading.Tasks.Task.Factory.StartNew(Sub() ProcessTag1(tag1, cpuType, sndrIndex, stringValues(0), dataType, bitIndex, chbBDOneZero.Checked, chbBDOnOff.Checked))
        Else
            AddressList(sndrIndex).ValuesToWrite.Text = "Busy Error"
            AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon

            'Set the corresponding status label back color to Red = Failed
            AddressList(sndrIndex).LabelStatus.BackColor = Color.Red
            Exit Sub
        End If
    End Sub

    Private Function ProcessTag1(tag1 As LibplctagWrapper.Tag, cpuType As LibplctagWrapper.CpuType, sndrIndex As Integer, PlcAddress As String, DataType As String, bitIndex As Integer, BDOneZero As Boolean, BDOnOff As Boolean) As Boolean
        Master.AddTag(tag1, tbTimeout.Text)

        While Master.GetStatus(tag1) = LibplctagWrapper.Libplctag.PLCTAG_STATUS_PENDING
            Threading.Thread.Sleep(10)
        End While

        ' if the status is not ok, we have to handle the error
        If Master.GetStatus(tag1) <> LibplctagWrapper.Libplctag.PLCTAG_STATUS_OK Then
            AddressList(sndrIndex).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(sndrIndex).ValuesToWrite.BackColor = Color.Salmon, MethodInvoker))
            AddressList(sndrIndex).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(sndrIndex).ValuesToWrite.Text = "Error", MethodInvoker))
            lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = "Error - " & Master.DecodeError(Master.GetStatus(tag1)), MethodInvoker))
            Master.RemoveTag(tag1)

            ' Set the corresponding status label back color to Red = Failed
            AddressList(sndrIndex).LabelStatus.Invoke(DirectCast(Sub() AddressList(sndrIndex).LabelStatus.BackColor = Color.Red, MethodInvoker))
            Return False
        End If

        Dim elementShift1 As Integer

        '*******************
        '*    R E A D      *
        '*******************

        If AddressList(sndrIndex).CheckBoxRead.Checked Then
            ' Set the corresponding status label back color to Yellow = Processing
            AddressList(sndrIndex).LabelStatus.BackColor = Color.Yellow

            Try
                Dim count As Integer

                If PlcAddress.IndexOfAny("/") <> -1 Then
                    count = 1
                Else
                    count = elementCount
                End If

                For i = 0 To count - 1
                    If Not (PlcAddress.IndexOfAny("/") <> -1 OrElse DataType = "BOOL" OrElse DataType = "BOOL Array" OrElse DataType = "Timer" OrElse DataType = "Counter" OrElse DataType = "Control") Then
                        Select Case DataType
                            Case "Int8", "SINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    If chbSwapBytes.Checked Then
                                        Dim quot, remainder As Integer
                                        If i > 0 Then quot = Math.DivRem(i, 2, remainder)

                                        If i = count - 1 Then
                                            If remainder = 0 Then
                                                StrMessage &= Master.GetInt8Value(tag1, i + 1)
                                            Else
                                                StrMessage &= Master.GetInt8Value(tag1, i - 1)
                                            End If
                                        Else
                                            If remainder = 0 Then
                                                StrMessage &= Master.GetInt8Value(tag1, i + 1) & ", "
                                            Else
                                                StrMessage &= Master.GetInt8Value(tag1, i - 1) & ", "
                                            End If
                                        End If
                                    Else
                                        If i = count - 1 Then
                                            StrMessage &= Master.GetInt8Value(tag1, i)
                                        Else
                                            StrMessage &= Master.GetInt8Value(tag1, i) & ", "
                                        End If
                                    End If
                                Else
                                    If i = count - 1 Then
                                        StrMessage &= Master.GetInt8Value(tag1, i * tag1.ElementSize)
                                    Else
                                        StrMessage &= Master.GetInt8Value(tag1, i * tag1.ElementSize) & ", "
                                    End If
                                End If
                            Case "UInt8", "USINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    If chbSwapBytes.Checked Then
                                        Dim quot, remainder As Integer
                                        If i > 0 Then quot = Math.DivRem(i, 2, remainder)

                                        If i = count - 1 Then
                                            If remainder = 0 Then
                                                StrMessage &= Master.GetUint8Value(tag1, i + 1)
                                            Else
                                                StrMessage &= Master.GetUint8Value(tag1, i - 1)
                                            End If
                                        Else
                                            If remainder = 0 Then
                                                StrMessage &= Master.GetUint8Value(tag1, i + 1) & ", "
                                            Else
                                                StrMessage &= Master.GetUint8Value(tag1, i - 1) & ", "
                                            End If
                                        End If
                                    Else
                                        If i = count - 1 Then
                                            StrMessage &= Master.GetUint8Value(tag1, i)
                                        Else
                                            StrMessage &= Master.GetUint8Value(tag1, i) & ", "
                                        End If
                                    End If
                                Else
                                    If i = count - 1 Then
                                        StrMessage &= Master.GetUint8Value(tag1, i * tag1.ElementSize)
                                    Else
                                        StrMessage &= Master.GetUint8Value(tag1, i * tag1.ElementSize) & ", "
                                    End If
                                End If
                            Case "Int16", "INT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetInt16Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetInt16Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "UInt16", "UINT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetUint16Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetUint16Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "Int32", "DINT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetInt32Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetInt32Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "UInt32", "UDINT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetUint32Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetUint32Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "Float32", "REAL"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetFloat32Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetFloat32Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "Int64", "LINT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetInt64Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetInt64Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "UInt64", "ULINT"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetUint64Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetUint64Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "Float64", "LREAL"
                                If i = count - 1 Then
                                    StrMessage &= Master.GetFloat64Value(tag1, i * tag1.ElementSize)
                                Else
                                    StrMessage &= Master.GetFloat64Value(tag1, i * tag1.ElementSize) & ", "
                                End If
                            Case "Int128", "QDINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    Dim strBytes = New Byte(15) {}

                                    For k = 0 To 15
                                        strBytes(k) = Master.GetUint8Value(tag1, k + i * tag1.ElementSize * 8)
                                    Next

                                    strBytes = SwapCheck(strBytes)

                                    If i = count - 1 Then
                                        StrMessage &= BitConverterInt128(BigInteger2BinaryString(strBytes)).ToString
                                    Else
                                        StrMessage &= BitConverterInt128(BigInteger2BinaryString(strBytes)).ToString & ", "
                                    End If
                                Else
                                    If i = count - 1 Then
                                        StrMessage &= Master.GetInt128Value(tag1, i * tag1.ElementSize).ToString
                                    Else
                                        StrMessage &= Master.GetInt128Value(tag1, i * tag1.ElementSize).ToString & ", "
                                    End If
                                End If
                            Case "UInt128", "QUDINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    Dim strBytes = New Byte(15) {}

                                    For k = 0 To 15
                                        strBytes(k) = Master.GetUint8Value(tag1, k + i * tag1.ElementSize * 8)
                                    Next

                                    strBytes = SwapCheck(strBytes)

                                    If i = count - 1 Then
                                        StrMessage &= BitConverterUInt128(BigInteger2BinaryString(strBytes)).ToString
                                    Else
                                        StrMessage &= BitConverterUInt128(BigInteger2BinaryString(strBytes)).ToString & ", "
                                    End If
                                Else
                                    If i = count - 1 Then
                                        StrMessage &= Master.GetUint128Value(tag1, i * tag1.ElementSize).ToString
                                    Else
                                        StrMessage &= Master.GetUint128Value(tag1, i * tag1.ElementSize).ToString & ", "
                                    End If
                                End If
                            Case "PID"
                                If PIDSuffix <> "" Then
                                    Select Case PIDSuffix
                                        Case "EN"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 15), BDOneZero, BDOnOff)
                                        Case "DN"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 13), BDOneZero, BDOnOff)
                                        Case "PV"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 12), BDOneZero, BDOnOff)
                                        Case "SP"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 11), BDOneZero, BDOnOff)
                                        Case "LL"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 10), BDOneZero, BDOnOff)
                                        Case "UL"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 9), BDOneZero, BDOnOff)
                                        Case "DB"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 8), BDOneZero, BDOnOff)
                                        Case "DA"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 7), BDOneZero, BDOnOff)
                                        Case "TF"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 6), BDOneZero, BDOnOff)
                                        Case "SC"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 5), BDOneZero, BDOnOff)
                                        Case "RG"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 4), BDOneZero, BDOnOff)
                                        Case "OL"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 3), BDOneZero, BDOnOff)
                                        Case "CM"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 2), BDOneZero, BDOnOff)
                                        Case "AM"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 1), BDOneZero, BDOnOff)
                                        Case "TM"
                                            StrMessage = BooleanDisplay(Master.GetBitValue(tag1, 0), BDOneZero, BDOnOff)
                                        Case "SPS"
                                            StrMessage = Master.GetInt16Value(tag1, 2 * tag1.ElementSize)
                                        Case "KC"
                                            StrMessage = Master.GetInt16Value(tag1, 3 * tag1.ElementSize)
                                        Case "Ti"
                                            StrMessage = Master.GetInt16Value(tag1, 4 * tag1.ElementSize)
                                        Case "TD"
                                            StrMessage = Master.GetInt16Value(tag1, 5 * tag1.ElementSize)
                                        Case "MAXS"
                                            StrMessage = Master.GetInt16Value(tag1, 7 * tag1.ElementSize)
                                        Case "MINS"
                                            StrMessage = Master.GetInt16Value(tag1, 8 * tag1.ElementSize)
                                        Case "ZCD"
                                            StrMessage = Master.GetInt16Value(tag1, 9 * tag1.ElementSize)
                                        Case "CVH"
                                            StrMessage = Master.GetInt16Value(tag1, 11 * tag1.ElementSize)
                                        Case "CVL"
                                            StrMessage = Master.GetInt16Value(tag1, 12 * tag1.ElementSize)
                                        Case "LUT"
                                            StrMessage = Master.GetInt16Value(tag1, 13 * tag1.ElementSize)
                                        Case "SPV"
                                            StrMessage = Master.GetInt16Value(tag1, 14 * tag1.ElementSize)
                                        Case "CVP"
                                            StrMessage = Master.GetInt16Value(tag1, 16 * tag1.ElementSize)
                                    End Select
                                Else
                                    For j = 0 To 22
                                        If j = 22 Then
                                            StrMessage &= Master.GetInt16Value(tag1, j * tag1.ElementSize)
                                        Else
                                            StrMessage &= Master.GetInt16Value(tag1, j * tag1.ElementSize) & ", "
                                        End If
                                    Next
                                End If
                            Case "Custom String"
                                'Actual String Length from first 4 bytes
                                Dim actStrLgth As Integer = Master.GetInt32Value(tag1, i * tag1.ElementSize)

                                Dim valUShort() = New String(CustomStringLength - 1) {}

                                For k = 0 To actStrLgth - 1
                                    valUShort(k) = Master.GetUint8Value(tag1, k + 4 + i * tag1.ElementSize).ToString
                                Next

                                If i = count - 1 Then
                                    StrMessage &= ConvertStringOfIntegersToString(valUShort)
                                Else
                                    StrMessage &= ConvertStringOfIntegersToString(valUShort) & ", "
                                End If
                            Case "String"
                                Dim valUShort() As String

                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    valUShort = New String(CustomStringLength - 1) {}
                                    Dim valBytes = New Byte(CustomStringLength - 1) {}

                                    For k = 0 To CustomStringLength - 1
                                        valBytes(k) = Master.GetUint8Value(tag1, k + i * tag1.ElementSize)
                                    Next

                                    valBytes = SwapCheck(valBytes)

                                    For k = 0 To CustomStringLength - 1
                                        valUShort(k) = valBytes(k).ToString
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                    'String Length from first byte
                                    Dim strLgth = Master.GetUint8Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    For k = 0 To strLgth - 1
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 1 + i * tag1.ElementSize).ToString
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                    'String Length from first 4 bytes
                                    Dim strLgth = Master.GetUint32Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    For k = 0 To strLgth - 1
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 4 + i * tag1.ElementSize).ToString
                                    Next
                                Else
                                    'String Length from first 2 bytes
                                    Dim strLgth = Master.GetUint16Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    Dim result As Integer
                                    Dim quot = Math.DivRem(strLgth, 2, result)

                                    If result = 0 Then
                                        valUShort = New String(strLgth - 1) {}
                                    Else
                                        valUShort = New String(strLgth) {}
                                    End If

                                    'Reverse bytes
                                    For k = 0 To valUShort.Length - 1 Step 2
                                        valUShort(k + 1) = Master.GetUint8Value(tag1, k + 2 + i * tag1.ElementSize).ToString
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 3 + i * tag1.ElementSize).ToString
                                    Next
                                End If

                                Dim str = ConvertStringOfIntegersToString(valUShort)

                                If i = count - 1 Then
                                    If str = "" Then
                                        StrMessage &= "{}"
                                    Else
                                        StrMessage &= str
                                    End If
                                Else
                                    If str = "" Then
                                        StrMessage &= "{}" & ", "
                                    Else
                                        StrMessage &= str & ", "
                                    End If
                                End If
                        End Select
                    Else
                        If PlcAddress.IndexOfAny("/") <> -1 Then 'Bit or Character Reading
                            If DataType = "Custom String" Then
                                ' Read the whole string, get the substring and show its characters separated by a comma

                                'Actual String Length from first 4 bytes
                                Dim actStrLgth As Integer = Master.GetInt32Value(tag1, i * tag1.ElementSize)

                                Dim valUShort() = New String(CustomStringLength - 1) {}

                                For k = 0 To actStrLgth - 1
                                    valUShort(k) = Master.GetUint8Value(tag1, k + 4 + i * tag1.ElementSize).ToString
                                Next

                                Dim finalString = ConvertStringOfIntegersToString(valUShort)

                                For m = bitIndex - 1 To bitIndex + elementCount - 2
                                    If m = bitIndex + elementCount - 2 Then
                                        StrMessage += finalString.Substring(m, 1)
                                    Else
                                        StrMessage += finalString.Substring(m, 1) + ", "
                                    End If
                                Next
                            ElseIf DataType = "String" Then
                                Dim valUShort() As String = Nothing

                                ' Read the whole string, get the substring and show its characters separated by a comma

                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    valUShort = New String(CustomStringLength - 1) {}
                                    Dim valBytes = New Byte(CustomStringLength - 1) {}

                                    For k = 0 To CustomStringLength - 1
                                        valBytes(k) = Master.GetUint8Value(tag1, k + i * tag1.ElementSize)
                                    Next

                                    valBytes = SwapCheck(valBytes)

                                    For k = 0 To CustomStringLength - 1
                                        valUShort(k) = valBytes(k).ToString
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                    'String Length from first byte
                                    Dim strLgth = Master.GetUint8Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    For k = 0 To strLgth - 1
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 1 + i * tag1.ElementSize).ToString
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                    'String Length from first 4 bytes
                                    Dim strLgth = Master.GetUint32Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    For k = 0 To strLgth - 1
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 4 + i * tag1.ElementSize).ToString
                                    Next
                                Else 'SLC Or PLC5
                                    'String Length from first 2 bytes
                                    Dim strLgth = Master.GetUint16Value(tag1, i * tag1.ElementSize)

                                    valUShort = New String(strLgth - 1) {}

                                    Dim result As Integer
                                    Dim quot = Math.DivRem(strLgth, 2, result)

                                    If result = 0 Then
                                        valUShort = New String(strLgth - 1) {}
                                    Else
                                        valUShort = New String(strLgth) {}
                                    End If

                                    For k = 0 To valUShort.Length - 1 Step 2 'Reverse bytes
                                        valUShort(k + 1) = Master.GetUint8Value(tag1, k + 2 + i * tag1.ElementSize).ToString
                                        valUShort(k) = Master.GetUint8Value(tag1, k + 3 + i * tag1.ElementSize).ToString
                                    Next
                                End If

                                Dim finalString = ConvertStringOfIntegersToString(valUShort)

                                If bitIndex > finalString.Length - 1 Then
                                    StrMessage += ""
                                Else
                                    For m = bitIndex - 1 To bitIndex + elementCount - 2
                                        If m = bitIndex + elementCount - 2 Then
                                            StrMessage += finalString.Substring(m, 1)
                                        Else
                                            StrMessage += finalString.Substring(m, 1) + ", "
                                        End If
                                    Next
                                End If
                            Else
                                If cpuType = 7 Then
                                    Select Case DataType
                                        Case "Int8", "SINT", "UInt8", "USINT"
                                            For j = 0 To elementCount - 1
                                                If j = elementCount - 1 Then
                                                    StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff)
                                                Else
                                                    StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        Case "Int16", "INT", "UInt16", "UINT"
                                            If chbSwapBytes.Checked Then
                                                Dim stringPart0 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 0), 2).PadLeft(8, "0"c))
                                                Dim stringPart1 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 1), 2).PadLeft(8, "0"c))
                                                Dim binaryString = stringPart1 & stringPart0

                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            Else
                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            End If
                                        Case "Int32", "DINT", "UInt32", "UDINT", "Float32", "REAL"
                                            If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                                Dim stringPart0 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 0), 2).PadLeft(8, "0"c))
                                                Dim stringPart1 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 1), 2).PadLeft(8, "0"c))
                                                Dim stringPart2 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 2), 2).PadLeft(8, "0"c))
                                                Dim stringPart3 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 3), 2).PadLeft(8, "0"c))
                                                Dim binaryString As String

                                                If chbSwapBytes.Checked Then
                                                    If chbSwapWords.Checked Then
                                                        binaryString = stringPart3 & stringPart2 & stringPart1 & stringPart0
                                                    Else
                                                        binaryString = stringPart1 & stringPart0 & stringPart3 & stringPart2
                                                    End If
                                                Else
                                                    binaryString = stringPart2 & stringPart3 & stringPart0 & stringPart1
                                                End If

                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            Else
                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            End If
                                        Case "Int64", "LINT", "UInt64", "ULINT", "Float644", "LREAL"
                                            If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                                Dim stringPart0 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 0), 2).PadLeft(8, "0"c))
                                                Dim stringPart1 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 1), 2).PadLeft(8, "0"c))
                                                Dim stringPart2 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 2), 2).PadLeft(8, "0"c))
                                                Dim stringPart3 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 3), 2).PadLeft(8, "0"c))
                                                Dim stringPart4 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 4), 2).PadLeft(8, "0"c))
                                                Dim stringPart5 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 5), 2).PadLeft(8, "0"c))
                                                Dim stringPart6 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 6), 2).PadLeft(8, "0"c))
                                                Dim stringPart7 = StrReverse(Convert.ToString(Master.GetUint8Value(tag1, 7), 2).PadLeft(8, "0"c))
                                                Dim binaryString As String

                                                If chbSwapBytes.Checked Then
                                                    If chbSwapWords.Checked Then
                                                        binaryString = stringPart7 & stringPart6 & stringPart5 & stringPart4 & stringPart3 & stringPart2 & stringPart1 & stringPart0
                                                    Else
                                                        binaryString = stringPart1 & stringPart0 & stringPart3 & stringPart2 & stringPart5 & stringPart4 & stringPart7 & stringPart6
                                                    End If
                                                Else
                                                    binaryString = stringPart6 & stringPart7 & stringPart4 & stringPart5 & stringPart2 & stringPart3 & stringPart0 & stringPart1
                                                End If

                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(ConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            Else
                                                For j = 0 To elementCount - 1
                                                    If j = elementCount - 1 Then
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff)
                                                    Else
                                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                    End If
                                                Next
                                            End If
                                    End Select
                                Else
                                    For j = 0 To elementCount - 1
                                        If j = elementCount - 1 Then
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff)
                                        Else
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                        End If
                                    Next
                                End If
                            End If
                        Else
                            Select Case DataType
                                Case "BOOL"
                                    If cpuType = 0 OrElse cpuType = 2 Then 'ControlLogix or Micro800
                                        StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, 0), BDOneZero, BDOnOff)
                                    ElseIf cpuType = 7 Then ' Modbus
                                        If i = count - 1 Then
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + i + 1), BDOneZero, BDOnOff)
                                        Else
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + i + 1), BDOneZero, BDOnOff) & ", "
                                        End If
                                    Else
                                        If i = count - 1 Then
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + i), BDOneZero, BDOnOff)
                                        Else
                                            StrMessage &= BooleanDisplay(Master.GetBitValue(tag1, bitIndex + i), BDOneZero, BDOnOff) & ", "
                                        End If
                                    End If
                                Case "BOOL Array"
                                    Dim remainder1 As Integer
                                    Dim startElement1 As Integer = Math.DivRem(bitIndex, tag1.ElementSize * 8, remainder1)

                                    If bitIndex + i > 0 Then
                                        Dim remainder2 As Integer
                                        Dim quotient As Integer = Math.DivRem(bitIndex + i, tag1.ElementSize * 8, remainder2)
                                        If remainder2 = 0 Then
                                            elementShift1 += 1
                                        End If
                                    End If

                                    If i = count - 1 Then
                                        StrMessage &= ExtractInt32Bit(Master.GetInt32Value(tag1, (startElement1 + elementShift1) * tag1.ElementSize), bitIndex - ((startElement1 + elementShift1) * tag1.ElementSize * 8) + i, BDOneZero, BDOnOff)
                                    Else
                                        StrMessage &= ExtractInt32Bit(Master.GetInt32Value(tag1, (startElement1 + elementShift1) * tag1.ElementSize), bitIndex - ((startElement1 + elementShift1) * tag1.ElementSize * 8) + i, BDOneZero, BDOnOff) & ", "
                                    End If
                                Case "Timer"
                                    If cpuType = LibplctagWrapper.CpuType.ControlLogix OrElse cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4)
                                            Else
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        If PlcAddress.IndexOfAny(".") <> -1 Then
                                            Dim tempVal = Master.GetInt16Value(tag1, i * tag1.ElementSize)

                                            Select Case PlcAddress.Substring(PlcAddress.IndexOf(".") + 1)
                                                Case "EN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Case "TT"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Case "DN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Case "PRE"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 2)
                                                Case "ACC"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 4)
                                            End Select
                                        Else
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2)
                                                Else
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2) & ", "
                                                End If
                                            Next
                                        End If
                                    End If
                                Case "Counter"
                                    If cpuType = LibplctagWrapper.CpuType.ControlLogix OrElse cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4)
                                            Else
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        If PlcAddress.IndexOfAny(".") <> -1 Then
                                            Dim tempVal = Master.GetInt16Value(tag1, i * elementSize)

                                            Select Case PlcAddress.Substring(PlcAddress.IndexOf(".") + 1)
                                                Case "CU"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Case "CD"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Case "DN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Case "OV"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff)
                                                Case "UN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff)
                                                Case "UA"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff)
                                                Case "PRE"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 2)
                                                Case "ACC"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 4)
                                            End Select
                                        Else
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2)
                                                Else
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2) & ", "
                                                End If
                                            Next
                                        End If
                                    End If
                                Case "Control"
                                    If cpuType = LibplctagWrapper.CpuType.ControlLogix OrElse cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4)
                                            Else
                                                StrMessage &= Master.GetInt32Value(tag1, i * elementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        If PlcAddress.IndexOfAny(".") <> -1 Then
                                            Dim tempVal = Master.GetInt16Value(tag1, i * elementSize)

                                            Select Case PlcAddress.Substring(PlcAddress.IndexOf(".") + 1)
                                                Case "EN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Case "EU"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Case "DN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Case "EM"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff)
                                                Case "ER"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff)
                                                Case "UL"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff)
                                                Case "IN"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 9, BDOneZero, BDOnOff)
                                                Case "FD"
                                                    StrMessage &= ExtractInt16Bit(tempVal, 8, BDOneZero, BDOnOff)
                                                Case "LEN"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 2)
                                                Case "POS"
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + 4)
                                            End Select
                                        Else
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2)
                                                Else
                                                    StrMessage &= Master.GetInt16Value(tag1, i * elementSize + j * 2) & ", "
                                                End If
                                            Next
                                        End If
                                    End If
                            End Select
                        End If
                    End If
                Next
            Catch ex As Exception
                MessageBox.Show(ex.Message)
                lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = ex.Message, MethodInvoker))
                Master.RemoveTag(tag1)
                StrMessage = ""

                'Set the corresponding status label back color to Red = Failed
                AddressList(sndrIndex).LabelStatus.Invoke(DirectCast(Sub() AddressList(sndrIndex).LabelStatus.BackColor = Color.Red, MethodInvoker))

                Return False
            End Try

            AddressList(sndrIndex).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(sndrIndex).ValuesToWrite.BackColor = Color.White, MethodInvoker))
            AddressList(sndrIndex).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(sndrIndex).ValuesToWrite.Text = StrMessage, MethodInvoker))
            StrMessage = ""

            '*******************
            '*   W R I T E     *
            '*******************
        Else
            If Not CheckWriteValues(sndrIndex) Then ' Check the values to write
                Master.RemoveTag(tag1)
                Return False
            End If

            'Set the corresponding status label back color to Yellow = Processing
            AddressList(sndrIndex).LabelStatus.BackColor = Color.Yellow

            'Split ValuesToWrite string based on comma delimiter
            Dim writeValues = AddressList(sndrIndex).ValuesToWrite.Text.Split(New Char() {","c})

            For k = 0 To writeValues.Length - 1
                writeValues(k) = writeValues(k).Trim
            Next

            Try
                If Not (PlcAddress.IndexOfAny("/") <> -1 OrElse DataType = "BOOL" OrElse DataType = "BOOL Array") Then
                    For i = 0 To elementCount - 1
                        Dim value2write = ""

                        If writeValues.Length = 1 Then
                            value2write = writeValues(0)
                        Else
                            value2write = writeValues(i)
                        End If

                        Select Case DataType
                            Case "Int8", "SINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS AndAlso chbSwapWords.Checked Then
                                    Master.SetInt8Value(tag1, (i + 1) * tag1.ElementSize, Convert.ToSByte(value2write))
                                Else
                                    Master.SetInt8Value(tag1, i * tag1.ElementSize, Convert.ToSByte(value2write))
                                End If
                            Case "UInt8", "USINT"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS AndAlso chbSwapWords.Checked Then
                                    Master.SetUint8Value(tag1, (i + 1) * tag1.ElementSize, Convert.ToByte(value2write))
                                Else
                                    Master.SetUint8Value(tag1, i * tag1.ElementSize, Convert.ToByte(value2write))
                                End If
                            Case "Int16", "INT"
                                Master.SetInt16Value(tag1, i * tag1.ElementSize, Convert.ToInt16(value2write))
                            Case "UInt16", "UINT"
                                Master.SetUint16Value(tag1, i * tag1.ElementSize, Convert.ToUInt16(value2write))
                            Case "Int32", "DINT"
                                Master.SetInt32Value(tag1, i * tag1.ElementSize, Convert.ToInt32(value2write))
                            Case "UInt32", "UDINT"
                                Master.SetUint32Value(tag1, i * tag1.ElementSize, Convert.ToUInt32(value2write))
                            Case "Float32", "REAL"
                                Master.SetFloat32Value(tag1, i * tag1.ElementSize, Convert.ToSingle(value2write))
                            Case "Int64", "LINT"
                                Master.SetInt64Value(tag1, i * tag1.ElementSize, Convert.ToInt64(value2write))
                            Case "UInt64", "ULINT"
                                Master.SetUint64Value(tag1, i * tag1.ElementSize, Convert.ToUInt64(value2write))
                            Case "Float64", "LREAL"
                                Master.SetFloat64Value(tag1, i * tag1.ElementSize, Convert.ToDouble(value2write))
                            Case "Int128", "QDINT"
                                Master.SetInt128Value(tag1, i * tag1.ElementSize, BigInteger.Parse(value2write))
                            Case "UInt128", "QUDINT"
                                Master.SetUint128Value(tag1, i * tag1.ElementSize, BigInteger.Parse(value2write))
                            Case "PID"
                                If PIDSuffix <> "" Then
                                    Select Case PIDSuffix
                                        Case "PV"
                                            Master.SetBitValue(tag1, 12, value2write)
                                        Case "SP"
                                            Master.SetBitValue(tag1, 11, value2write)
                                        Case "LL"
                                            Master.SetBitValue(tag1, 10, value2write)
                                        Case "UL"
                                            Master.SetBitValue(tag1, 9, value2write)
                                        Case "DB"
                                            Master.SetBitValue(tag1, 8, value2write)
                                        Case "DA"
                                            Master.SetBitValue(tag1, 7, value2write)
                                        Case "TF"
                                            Master.SetBitValue(tag1, 6, value2write)
                                        Case "SC"
                                            Master.SetBitValue(tag1, 5, value2write)
                                        Case "RG"
                                            Master.SetBitValue(tag1, 4, value2write)
                                        Case "OL"
                                            Master.SetBitValue(tag1, 3, value2write)
                                        Case "CM"
                                            Master.SetBitValue(tag1, 2, value2write)
                                        Case "AM"
                                            Master.SetBitValue(tag1, 1, value2write)
                                        Case "TM"
                                            Master.SetBitValue(tag1, 0, value2write)
                                        Case "SPS"
                                            Master.SetInt16Value(tag1, 2 * tag1.ElementSize, value2write)
                                        Case "KC"
                                            Master.SetInt16Value(tag1, 3 * tag1.ElementSize, value2write)
                                        Case "Ti"
                                            Master.SetInt16Value(tag1, 4 * tag1.ElementSize, value2write)
                                        Case "TD"
                                            Master.SetInt16Value(tag1, 5 * tag1.ElementSize, value2write)
                                        Case "MAXS"
                                            Master.SetInt16Value(tag1, 7 * tag1.ElementSize, value2write)
                                        Case "MINS"
                                            Master.SetInt16Value(tag1, 8 * tag1.ElementSize, value2write)
                                        Case "ZCD"
                                            Master.SetInt16Value(tag1, 9 * tag1.ElementSize, value2write)
                                        Case "CVH"
                                            Master.SetInt16Value(tag1, 11 * tag1.ElementSize, value2write)
                                        Case "CVL"
                                            Master.SetInt16Value(tag1, 12 * tag1.ElementSize, value2write)
                                        Case "LUT"
                                            Master.SetInt16Value(tag1, 13 * tag1.ElementSize, value2write)
                                        Case "SPV"
                                            Master.SetInt16Value(tag1, 14 * tag1.ElementSize, value2write)
                                        Case "CVP"
                                            Master.SetInt16Value(tag1, 16 * tag1.ElementSize, value2write)
                                    End Select
                                End If
                            Case "Custom String"
                                Dim data = ConvertStringToStringOfUShorts(value2write)
                                Dim valUShort(CustomStringLength + 3) As UShort
                                Dim bytes = BitConverter.GetBytes(data.Length)

                                bytes.CopyTo(valUShort, 0)
                                data.CopyTo(valUShort, 4)

                                For j = 0 To valUShort.Length - 1
                                    Master.SetUint8Value(tag1, j + i * tag1.ElementSize, valUShort(j))
                                Next
                            Case "String"
                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    Dim data = ConvertStringToStringOfUShorts(value2write)
                                    Dim valUShort(CustomStringLength - 1) As UShort
                                    data.CopyTo(valUShort, 0)

                                    valUShort = SwapCheckUshort(valUShort)

                                    For j = 0 To valUShort.Length - 1
                                        Master.SetUint8Value(tag1, j + i * tag1.ElementSize, valUShort(j))
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                    Dim data = ConvertStringToStringOfUShorts(value2write)
                                    Dim valUShort(data.Length) As UShort
                                    Dim bytes = BitConverter.GetBytes(data.Length)
                                    bytes.CopyTo(valUShort, 0)
                                    data.CopyTo(valUShort, 1)

                                    For j = 0 To valUShort.Length - 1
                                        Master.SetUint8Value(tag1, j + i * tag1.ElementSize, valUShort(j))
                                    Next
                                ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                    Dim data = ConvertStringToStringOfUShorts(value2write)
                                    Dim valUShort(data.Length + 3) As UShort
                                    Dim bytes = BitConverter.GetBytes(data.Length)
                                    bytes.CopyTo(valUShort, 0)
                                    data.CopyTo(valUShort, 4)

                                    For j = 0 To valUShort.Length - 1
                                        Master.SetUint8Value(tag1, j + i * tag1.ElementSize, valUShort(j))
                                    Next
                                Else
                                    Dim result As Integer
                                    Dim quot = Math.DivRem(value2write.Length, 2, result)

                                    Dim offset = 0

                                    If result <> 0 Then
                                        offset = 1
                                    End If

                                    Dim data = ConvertStringToStringOfUShorts(value2write)

                                    Dim valUShort(83) As UShort

                                    valUShort(0) = Convert.ToByte(BitConverter.GetBytes(data.Length)(0))
                                    valUShort(1) = Convert.ToByte(BitConverter.GetBytes(data.Length)(1))

                                    data.CopyTo(valUShort, 2)

                                    ' Reverse data bytes
                                    For z = 2 To data.Length + offset Step 2
                                        Dim temp As UShort = valUShort(z)
                                        valUShort(z) = valUShort(z + 1)
                                        valUShort(z + 1) = temp
                                    Next

                                    For j = 0 To valUShort.Length - 1
                                        Master.SetUint8Value(tag1, j + i * tag1.ElementSize, valUShort(j))
                                    Next
                                End If
                        End Select

                        Master.WriteTag(tag1, tbTimeout.Text)
                    Next
                Else
                    Dim boolValueToWrite As Boolean

                    If DataType = "BOOL" Then
                        For i = 0 To elementCount - 1
                            If writeValues.Length = 1 Then
                                If writeValues(0) = "0" OrElse writeValues(0) = "False" OrElse writeValues(0) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            Else
                                If writeValues(i) = "0" OrElse writeValues(i) = "False" OrElse writeValues(i) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            End If

                            If cpuType = LibplctagWrapper.CpuType.ControlLogix OrElse cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                If boolValueToWrite Then
                                    Master.SetUint8Value(tag1, i, 1)
                                Else
                                    Master.SetUint8Value(tag1, i, 0)
                                End If
                            Else
                                Master.SetBitValue(tag1, i, boolValueToWrite)
                            End If
                        Next
                    ElseIf DataType = "BOOL Array" AndAlso bitIndex + elementCount < 32 Then
                        For i = 0 To elementCount - 1
                            If writeValues.Length = 1 Then
                                If writeValues(0) = "0" OrElse writeValues(0) = "False" OrElse writeValues(0) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            Else
                                If writeValues(i) = "0" OrElse writeValues(i) = "False" OrElse writeValues(i) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            End If

                            Master.WriteBool(tag1, bitIndex + i, boolValueToWrite)
                        Next
                    ElseIf DataType = "BOOL Array" AndAlso bitIndex + elementCount >= 32 Then ' Read tag - Change bits - Write tag
                        For i = 0 To elementCount - 1
                            If writeValues.Length = 1 Then
                                If writeValues(0) = "0" OrElse writeValues(0) = "False" OrElse writeValues(0) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            Else
                                If writeValues(i) = "0" OrElse writeValues(i) = "False" OrElse writeValues(i) = "false" Then
                                    boolValueToWrite = False
                                Else
                                    boolValueToWrite = True
                                End If
                            End If

                            Dim remainder1 As Integer
                            Dim startElement1 As Integer = Math.DivRem(bitIndex, tag1.ElementSize * 8, remainder1)
                            If bitIndex + i > 0 Then
                                Dim remainder2 As Integer
                                Dim quotient As Integer = Math.DivRem(bitIndex + i, tag1.ElementSize * 8, remainder2)
                                If remainder2 = 0 Then
                                    elementShift1 += 1
                                End If
                            End If

                            Dim modifiedValue = ChangeInt32Bit(Master.GetInt32Value(tag1, (startElement1 + elementShift1) * tag1.ElementSize), bitIndex - ((startElement1 + elementShift1) * tag1.ElementSize * 8) + i, boolValueToWrite)
                            Master.SetInt32Value(tag1, (startElement1 + elementShift1) * tag1.ElementSize, modifiedValue)
                        Next
                    ElseIf PlcAddress.IndexOfAny("/") <> -1 Then
                        If DataType = "Custom String" Then
                            Dim valUShort() As String = New String(CustomStringLength - 1) {}
                            Dim finalString As String

                            For k = 0 To CustomStringLength - 1
                                valUShort(k) = Master.GetUint8Value(tag1, k + 4).ToString
                            Next
                            finalString = ConvertStringOfIntegersToString(valUShort)

                            Dim substringToChange As String = ""

                            For z = 0 To writeValues.Length - 1
                                substringToChange &= writeValues(z)
                            Next

                            finalString = finalString.Substring(0, bitIndex - 1) & substringToChange & finalString.Substring(bitIndex + elementCount - 1)

                            Dim valuesUShort() As UShort
                            Dim data = ConvertStringToStringOfUShorts(finalString)
                            valuesUShort = New UShort(data.Length + 3) {}
                            Dim bytes = BitConverter.GetBytes(data.Length)
                            bytes.CopyTo(valuesUShort, 0)
                            data.CopyTo(valuesUShort, 4)

                            For j = 0 To valuesUShort.Length - 1
                                Master.SetUint8Value(tag1, j, valuesUShort(j))
                            Next
                        ElseIf DataType = "String" Then ' Character Writing
                            Dim valUShort() As String
                            Dim finalString As String

                            ' Read the whole string, change the substring and write it back
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                valUShort = New String(CustomStringLength - 1) {}
                                Dim valBytes = New Byte(CustomStringLength - 1) {}

                                For k = 0 To CustomStringLength - 1
                                    valBytes(k) = Master.GetUint8Value(tag1, k)
                                Next

                                valBytes = SwapCheck(valBytes)

                                For k = 0 To CustomStringLength - 1
                                    valUShort(k) = valBytes(k).ToString
                                Next
                            ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                Dim strLgth As UShort 'String Length from first byte

                                strLgth = Master.GetUint8Value(tag1, 0)

                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = Master.GetUint8Value(tag1, k + 1).ToString
                                Next
                            ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                Dim bytes(3) As Byte

                                For j = 0 To 3
                                    bytes(j) = CByte(Master.GetUint8Value(tag1, j).ToString)
                                Next

                                Dim strLgth = BitConverter.ToInt32(bytes, 0)
                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = Master.GetUint8Value(tag1, k + 4).ToString
                                Next
                            Else ' SLC Or PLC5
                                Dim bytes(1) As Byte

                                For j = 0 To 1
                                    bytes(j) = CByte(Master.GetUint8Value(tag1, j).ToString)
                                Next

                                Dim strLgth = BitConverter.ToInt16(bytes, 0)
                                valUShort = New String(strLgth - 1) {}

                                Dim result As Integer
                                Dim quot = Math.DivRem(strLgth, 2, result)

                                If result = 0 Then
                                    valUShort = New String(strLgth - 1) {}
                                Else
                                    valUShort = New String(strLgth) {}
                                End If

                                For k = 0 To valUShort.Length - 1 Step 2 ' Reverse bytes
                                    valUShort(k + 1) = Master.GetUint8Value(tag1, k + 2).ToString
                                    valUShort(k) = Master.GetUint8Value(tag1, k + 3).ToString
                                Next
                            End If

                            finalString = ConvertStringOfIntegersToString(valUShort)

                            Dim substringToChange As String = ""

                            For z = 0 To writeValues.Length - 1
                                substringToChange &= writeValues(z)
                            Next

                            finalString = finalString.Substring(0, bitIndex - 1) & substringToChange & finalString.Substring(bitIndex + elementCount - 1)

                            Dim valuesUShort() As UShort
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                Dim data = ConvertStringToStringOfUShorts(finalString)
                                valuesUShort = New UShort(CustomStringLength - 1) {}
                                data.CopyTo(valuesUShort, 0)

                                valuesUShort = SwapCheckUshort(valuesUShort)
                            ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                Dim data = ConvertStringToStringOfUShorts(finalString)
                                valuesUShort = New UShort(data.Length) {}
                                Dim bytes = BitConverter.GetBytes(data.Length)
                                bytes.CopyTo(valuesUShort, 0)
                                data.CopyTo(valuesUShort, 1)
                            ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                Dim data = ConvertStringToStringOfUShorts(finalString)
                                valuesUShort = New UShort(data.Length + 3) {}
                                Dim bytes = BitConverter.GetBytes(data.Length)
                                bytes.CopyTo(valuesUShort, 0)
                                data.CopyTo(valuesUShort, 4)
                            Else
                                Dim result As Integer
                                Dim quot = Math.DivRem(finalString.Length, 2, result)

                                If result <> 0 Then
                                    finalString &= " "
                                End If

                                Dim data = ConvertStringToStringOfUShorts(finalString)
                                valuesUShort = New UShort(data.Length + 1) {}
                                valuesUShort(0) = Convert.ToByte(BitConverter.GetBytes(data.Length)(0))
                                valuesUShort(1) = Convert.ToByte(BitConverter.GetBytes(data.Length)(1))

                                ' Reverse data bytes
                                For z = 0 To data.Length - 1 Step 2
                                    Dim temp As UShort = data(z)
                                    data(z) = data(z + 1)
                                    data(z + 1) = temp
                                Next

                                data.CopyTo(valuesUShort, 2)
                            End If

                            For j = 0 To valuesUShort.Length - 1
                                Master.SetUint8Value(tag1, j, valuesUShort(j))
                            Next
                        Else  ' Bit Writing
                            For i = 0 To elementCount - 1
                                If writeValues.Length = 1 Then
                                    If writeValues(0) = "0" OrElse writeValues(0) = "False" OrElse writeValues(0) = "false" Then
                                        boolValueToWrite = False
                                    Else
                                        boolValueToWrite = True
                                    End If
                                Else
                                    If writeValues(i) = "0" OrElse writeValues(i) = "False" OrElse writeValues(i) = "false" Then
                                        boolValueToWrite = False
                                    Else
                                        boolValueToWrite = True
                                    End If
                                End If

                                If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                    Select Case DataType
                                        Case "Int8", "UInt8"
                                            If chbSwapBytes.Checked Then
                                                Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                            Else
                                                Master.SetBitValue(tag1, bitIndex + i, boolValueToWrite)
                                            End If
                                        Case "Int16", "UInt16"
                                            If chbSwapBytes.Checked Then
                                                If bitIndex < 8 Then
                                                    Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                Else
                                                    Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                End If
                                            Else
                                                Master.SetBitValue(tag1, bitIndex + i, boolValueToWrite)
                                            End If
                                        Case "Int32", "UInt32", "Float32"
                                            If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                                If chbSwapBytes.Checked Then
                                                    If chbSwapWords.Checked Then 'byte3 + byte2 + byte1 + byte0
                                                        If bitIndex < 8 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 24, boolValueToWrite)
                                                        ElseIf bitIndex < 16 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 24 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        Else
                                                            Master.SetBitValue(tag1, bitIndex + i - 24, boolValueToWrite)
                                                        End If
                                                    Else 'byte1 + byte0 + byte3 + byte2
                                                        If bitIndex < 8 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 16 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        ElseIf bitIndex < 24 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        Else
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        End If
                                                    End If
                                                Else 'byte2 + byte3 + byte0 + byte1
                                                    If bitIndex < 16 Then
                                                        Master.SetBitValue(tag1, bitIndex + i + 16, boolValueToWrite)
                                                    Else
                                                        Master.SetBitValue(tag1, bitIndex + i - 16, boolValueToWrite)
                                                    End If
                                                End If
                                            End If
                                        Case "Int64", "UInt64", "Float64"
                                            If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                                If chbSwapBytes.Checked Then
                                                    If chbSwapWords.Checked Then 'byte7 + byte6 + byte5 + byte4 + byte3 + byte2 + byte1 + byte0
                                                        If bitIndex < 8 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 56, boolValueToWrite)
                                                        ElseIf bitIndex < 16 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 40, boolValueToWrite)
                                                        ElseIf bitIndex < 24 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 24, boolValueToWrite)
                                                        ElseIf bitIndex < 32 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 40 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        ElseIf bitIndex < 48 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 24, boolValueToWrite)
                                                        ElseIf bitIndex < 56 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 40, boolValueToWrite)
                                                        Else
                                                            Master.SetBitValue(tag1, bitIndex + i - 56, boolValueToWrite)
                                                        End If
                                                    Else 'byte1 + byte0 + byte3 + byte2 + byte5 + byte4 + byte7 + byte6
                                                        If bitIndex < 8 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 16 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        ElseIf bitIndex < 24 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 32 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        ElseIf bitIndex < 40 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        ElseIf bitIndex < 48 Then
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        ElseIf bitIndex < 56 Then
                                                            Master.SetBitValue(tag1, bitIndex + i + 8, boolValueToWrite)
                                                        Else
                                                            Master.SetBitValue(tag1, bitIndex + i - 8, boolValueToWrite)
                                                        End If
                                                    End If
                                                Else 'byte6 + byte7 + byte4 + byte5 + byte2 + byte3 + byte0 + byte1
                                                    If bitIndex < 16 Then
                                                        Master.SetBitValue(tag1, bitIndex + i + 48, boolValueToWrite)
                                                    ElseIf bitIndex < 32 Then
                                                        Master.SetBitValue(tag1, bitIndex + i + 16, boolValueToWrite)
                                                    ElseIf bitIndex < 48 Then
                                                        Master.SetBitValue(tag1, bitIndex + i - 16, boolValueToWrite)
                                                    Else
                                                        Master.SetBitValue(tag1, bitIndex + i - 48, boolValueToWrite)
                                                    End If
                                                End If
                                            End If
                                    End Select
                                Else
                                    Master.SetBitValue(tag1, bitIndex + i, boolValueToWrite)
                                End If
                            Next
                        End If
                    End If

                    Master.WriteTag(tag1, tbTimeout.Text)
                End If
            Catch ex As Exception
                MessageBox.Show(ex.Message)
                lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = ex.Message, MethodInvoker))
                Master.RemoveTag(tag1)
                StrMessage = ""
                PIDSuffix = ""

                'Set the corresponding status label back color to Red = Failed
                AddressList(sndrIndex).LabelStatus.Invoke(DirectCast(Sub() AddressList(sndrIndex).LabelStatus.BackColor = Color.Red, MethodInvoker))

                Return False
            End Try
        End If

        Master.RemoveTag(tag1)

        CustomStringLength = 0
        PIDSuffix = ""

        'Set the corresponding status label back color to Green = Success
        AddressList(sndrIndex).LabelStatus.Invoke(DirectCast(Sub() AddressList(sndrIndex).LabelStatus.BackColor = Color.LimeGreen, MethodInvoker))

        AddressList(sndrIndex).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(sndrIndex).ValuesToWrite.BackColor = Color.White, MethodInvoker))

        lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = "Comms Okay", MethodInvoker))

        Tag1ProcessingTask = Nothing

        Return True
    End Function

#End Region

#Region "AutoRead"

    Private Sub CheckBoxAutoRead_CheckedChanged(sender As Object, e As EventArgs) Handles chbAutoRead.CheckedChanged
        If chbAutoRead.Checked Then
            cbPollInterval.Enabled = False

            Dim prot As String = cbProtocol.SelectedItem.ToString
            Dim ipAddress As String = tbIPAddress.Text

            For i = 0 To 11
                If AddressList(i).CheckBoxRead.Checked Then
                    'Split tag string based on semicolon delimiter (PLC Address, Data Type, Element Count, (Custom) String Length)
                    Dim stringValues = AddressList(i).PlcAddress.Text.Split(New Char() {";"c})

                    For j = 0 To stringValues.Length - 1
                        stringValues(j) = stringValues(j).Replace(" ", "") 'Remove all spaces
                    Next

                    Dim cpuType As LibplctagWrapper.CpuType

                    For Each typ As LibplctagWrapper.CpuType In [Enum].GetValues(GetType(LibplctagWrapper.CpuType))
                        If typ.ToString = cbCPUType.SelectedItem Then
                            cpuType = typ
                            Exit For
                        End If
                    Next

                    Dim AutoReadElementSize As Integer
                    Dim address2poll As String = stringValues(0)
                    Dim DataType As String = stringValues(1)
                    Dim AutoReadElementCount As Integer = stringValues(2)

                    Select Case DataType
                        Case "Int8", "SINT", "UInt8", "USINT", "BOOL"
                            AutoReadElementSize = 1

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If chbSwapBytes.Checked Then
                                    AutoReadbyteOrder = int16byteOrder(0)
                                Else
                                    AutoReadbyteOrder = int16byteOrder(1)
                                End If

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 1
                                Else
                                    RealElementCount2 = AutoReadElementCount
                                End If
                            End If
                        Case "Int16", "INT", "UInt16", "UINT"
                            AutoReadElementSize = 2

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                If chbSwapBytes.Checked Then
                                    AutoReadbyteOrder = int16byteOrder(0)
                                Else
                                    AutoReadbyteOrder = int16byteOrder(1)
                                End If

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 1
                                Else
                                    RealElementCount2 = AutoReadElementCount
                                End If
                            ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                                cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                                cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                                byteOrder = int16byteOrder(1)
                            End If
                        Case "Int32", "DINT", "UInt32", "UDINT", "BOOLArray"
                            AutoReadElementSize = 4

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 2
                                Else
                                    RealElementCount2 = AutoReadElementCount * 2
                                End If

                                If chbSwapBytes.Checked Then
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = int32byteOrder(0)
                                    Else
                                        AutoReadbyteOrder = int32byteOrder(2)
                                    End If
                                Else
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = int32byteOrder(1)
                                    Else
                                        AutoReadbyteOrder = int32byteOrder(3)
                                    End If
                                End If
                            ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                                cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                                cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                                byteOrder = int32byteOrder(3)
                            Else
                                If DataType = "BOOLArray" Then DataType = "BOOL Array"
                            End If
                        Case "Float32", "REAL"
                            AutoReadElementSize = 4

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 2
                                Else
                                    RealElementCount2 = AutoReadElementCount * 2
                                End If

                                If chbSwapBytes.Checked Then
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = float32byteOrder(0)
                                    Else
                                        AutoReadbyteOrder = float32byteOrder(2)
                                    End If
                                Else
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = float32byteOrder(1)
                                    Else
                                        AutoReadbyteOrder = float32byteOrder(3)
                                    End If
                                End If
                            ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                                cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                                byteOrder = float32byteOrder(3)
                            ElseIf (libplctagIsNew = 0) AndAlso cpuType = LibplctagWrapper.CpuType.Plc5 Then

                                byteOrder = float32byteOrder(1)
                            End If
                        Case "Int64", "LINT", "UInt64", "ULINT"
                            AutoReadElementSize = 8

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 4
                                Else
                                    RealElementCount2 = AutoReadElementCount * 4
                                End If

                                If chbSwapBytes.Checked Then
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = int64byteOrder(0)
                                    Else
                                        AutoReadbyteOrder = int64byteOrder(2)
                                    End If
                                Else
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = int64byteOrder(1)
                                    Else
                                        AutoReadbyteOrder = int64byteOrder(3)
                                    End If
                                End If
                            ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                                cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                                cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                                byteOrder = int64byteOrder(3)
                            End If
                        Case "Float64", "LREAL"
                            AutoReadElementSize = 8

                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 4
                                Else
                                    RealElementCount2 = AutoReadElementCount * 4
                                End If

                                If chbSwapBytes.Checked Then
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = float64byteOrder(0)
                                    Else
                                        AutoReadbyteOrder = float64byteOrder(2)
                                    End If
                                Else
                                    If chbSwapWords.Checked Then
                                        AutoReadbyteOrder = float64byteOrder(1)
                                    Else
                                        AutoReadbyteOrder = float64byteOrder(3)
                                    End If
                                End If
                            ElseIf (libplctagIsNew = 0) AndAlso (cpuType = LibplctagWrapper.CpuType.MicroLogix OrElse
                                cpuType = LibplctagWrapper.CpuType.Slc500 OrElse cpuType = LibplctagWrapper.CpuType.Plc5 OrElse
                                cpuType = LibplctagWrapper.CpuType.Logixpccc) Then

                                byteOrder = float64byteOrder(3)
                            End If
                        Case "Int128", "QDINT", "UInt128", "QUDINT"
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadElementSize = 2

                                If address2poll.IndexOfAny("/") <> -1 Then
                                    RealElementCount2 = 8
                                Else
                                    RealElementCount2 = AutoReadElementCount * 8
                                End If
                            Else
                                AutoReadElementSize = 16
                            End If
                        Case "PID" 'Only applicable to MicroLogix
                            AutoReadElementSize = 2
                            RealElementCount2 = 23

                            If address2poll.IndexOfAny(".") <> -1 Then
                                AutoReadPIDSuffix = stringValues(0).Substring(stringValues(0).IndexOf(".") + 1)
                                address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(".")) 'Workaround
                            End If
                        Case "CustomString" 'Only applicable to ControlLogix
                            DataType = "Custom String"
                            AutoReadCustomStringLength = stringValues(3)
                            AutoReadElementSize = Math.Ceiling(AutoReadCustomStringLength / 8.0F) * 8
                        Case "String"
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                AutoReadCustomStringLength = stringValues(3)
                                AutoReadElementSize = 2
                                RealElementCount2 = Math.Ceiling(AutoReadCustomStringLength / 2.0F)
                            ElseIf cpuType = LibplctagWrapper.CpuType.Micro800 Then
                                AutoReadElementSize = 256
                            ElseIf cpuType = LibplctagWrapper.CpuType.ControlLogix Then
                                AutoReadElementSize = 88
                            Else
                                AutoReadElementSize = 84
                            End If
                        Case Else 'Timer or Counter or Control
                            If cpuType = 0 OrElse cpuType = 2 Then
                                If stringValues(0).EndsWith(".PRE") OrElse stringValues(0).EndsWith(".ACC") OrElse
                                    stringValues(0).EndsWith(".LEN") OrElse stringValues(0).EndsWith(".POS") Then

                                    AutoReadElementSize = 4
                                    DataType = "DINT"
                                ElseIf stringValues(0).EndsWith(".EN") OrElse stringValues(0).EndsWith(".TT") OrElse
                                    stringValues(0).EndsWith(".DN") OrElse stringValues(0).EndsWith(".CU") OrElse
                                    stringValues(0).EndsWith(".CD") OrElse stringValues(0).EndsWith(".OV") OrElse
                                    stringValues(0).EndsWith(".UN") OrElse stringValues(0).EndsWith(".UA") OrElse
                                    stringValues(0).EndsWith(".EU") OrElse stringValues(0).EndsWith(".EM") OrElse
                                    stringValues(0).EndsWith(".ER") OrElse stringValues(0).EndsWith(".UL") OrElse
                                    stringValues(0).EndsWith(".IN") OrElse stringValues(0).EndsWith(".FD") Then

                                    AutoReadElementSize = 1
                                    DataType = "BOOL"
                                Else
                                    AutoReadElementSize = 12
                                End If
                            Else
                                If stringValues(0).EndsWith(".PRE") OrElse stringValues(0).EndsWith(".ACC") OrElse
                                    stringValues(0).EndsWith(".LEN") OrElse stringValues(0).EndsWith(".POS") Then

                                    AutoReadElementSize = 2
                                    DataType = "INT"
                                Else
                                    AutoReadElementSize = 6
                                End If
                            End If
                    End Select

                    Dim bitIndex As Integer = -1

                    Try
                        If stringValues(0).IndexOfAny("/") <> -1 Then
                            If stringValues(0).Length > stringValues(0).IndexOf("/") + 1 Then
                                bitIndex = Convert.ToInt32(stringValues(0).Substring(stringValues(0).IndexOf("/") + 1))
                            Else
                                AddressList(i).ValuesToWrite.Text = "No index specified"
                                AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                Exit Sub
                            End If

                            If DataType = "String" Then
                                If bitIndex < 1 OrElse (cpuType = 2 AndAlso bitIndex + AutoReadElementCount > 255) OrElse
                                    (cpuType = 0 AndAlso bitIndex + AutoReadElementCount > 84) OrElse
                                    (cpuType = 7 AndAlso bitIndex + AutoReadElementCount > AutoReadCustomStringLength) OrElse
                                    (cpuType <> 0 AndAlso cpuType <> 2 AndAlso cpuType <> 7 AndAlso bitIndex + AutoReadElementCount > 82) Then

                                    AddressList(i).ValuesToWrite.Text = "Out of range"
                                    AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                    Exit Sub
                                End If
                            ElseIf DataType = "Custom String" Then
                                If bitIndex < 1 OrElse bitIndex + AutoReadElementCount > AutoReadCustomStringLength Then
                                    AddressList(i).ValuesToWrite.Text = "Out of range"
                                    AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                    Exit Sub
                                End If
                            Else
                                If bitIndex < 0 OrElse (cpuType <> 7 AndAlso bitIndex + AutoReadElementCount > AutoReadElementSize * 8) OrElse (cpuType = 7 AndAlso bitIndex + AutoReadElementCount > AutoReadElementSize * RealElementCount2 * 8) Then
                                    AddressList(i).ValuesToWrite.Text = "Out of range"
                                    AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                    Exit Sub
                                End If
                            End If

                            address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf("/")) 'Workaround
                            If cpuType < 7 Then RealElementCount2 = 1

                        ElseIf DataType = "BOOL Array" Then
                            bitIndex = CInt(stringValues(0).Substring(stringValues(0).IndexOf("[") + 1, stringValues(0).IndexOf("]") - stringValues(0).IndexOf("[") - 1))
                            RealElementCount2 = Math.Ceiling((bitIndex + CInt(stringValues(2))) / (AutoReadElementSize * 8))
                            address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf("[")) & "[0]" 'Workaround

                        ElseIf stringValues(0).IndexOfAny("[") <> -1 AndAlso stringValues(0).IndexOfAny(",") <> -1 AndAlso stringValues(0).IndexOfAny("]") <> -1 Then
                            If stringValues(0).IndexOf(",") < stringValues(0).LastIndexOf(",") Then
                                If System.Text.RegularExpressions.Regex.Matches(stringValues(0), ",").Count > 2 Then
                                    AddressList(i).ValuesToWrite.Text = "Unsupported array"
                                    AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                    Exit Sub
                                Else
                                    'Array [x, y, z]
                                    address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(",")) & "][" & stringValues(0).Substring(stringValues(0).IndexOf(",") + 1, stringValues(0).LastIndexOf(",") - stringValues(0).IndexOf(",") - 1) & "][" & stringValues(0).Substring(stringValues(0).LastIndexOf(",") + 1) 'Workaround
                                End If
                            Else
                                'Array [x, y]
                                address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(",")) & "][" & stringValues(0).Substring(stringValues(0).IndexOf(",") + 1) 'Workaround
                            End If

                        ElseIf cpuType <> 0 AndAlso cpuType <> 2 AndAlso cpuType <> 6 AndAlso cpuType <> 7 AndAlso (DataType = "Timer" OrElse DataType = "Counter" OrElse DataType = "Control") AndAlso stringValues(0).IndexOfAny(".") <> -1 Then
                            address2poll = stringValues(0).Substring(0, stringValues(0).IndexOf(".")) 'Workaround
                        End If
                    Catch ex As Exception
                        lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = "Error - " & ex.Message, MethodInvoker))
                        Exit Sub
                    End Try

                    Dim path As String

                    If cpuType = 0 OrElse cpuType = 1 OrElse cpuType = 6 OrElse cpuType = 7 Then
                        If String.IsNullOrWhiteSpace(tbPath.Text) Then
                            If cpuType = 7 Then
                                AddressList(i).ValuesToWrite.Text = "UnitID not specified"
                                AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                Exit Sub
                            Else
                                AddressList(i).ValuesToWrite.Text = "Path not specified"
                                AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                Exit Sub
                            End If
                        End If

                        path = tbPath.Text.Replace(" ", "")

                        If cpuType = 7 Then
                            Dim dummy As Integer
                            If Integer.TryParse(path, dummy) Then
                                If dummy < 1 OrElse dummy > 247 Then
                                    AddressList(i).ValuesToWrite.Text = "Invalid UnitID"
                                    AddressList(i).ValuesToWrite.BackColor = Color.Salmon
                                    Exit Sub
                                End If
                            End If
                        End If

                        If DataType = "BOOL Array" OrElse stringValues(0).IndexOfAny("/") <> -1 OrElse cpuType = 7 Then
                            If AutoReadbyteOrder IsNot Nothing Then
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, AutoReadElementSize, RealElementCount2, AutoReadbyteOrder)
                                AutoReadbyteOrder = Nothing
                            Else
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, AutoReadElementSize, RealElementCount2)
                            End If
                        Else
                            tag2in = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, address2poll, AutoReadElementSize, AutoReadElementCount)
                        End If
                    Else
                        If stringValues(0).IndexOfAny("/") <> -1 OrElse DataType = "PID" Then
                            If (libplctagIsNew = 0) AndAlso byteOrder IsNot Nothing Then
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, AutoReadElementSize, RealElementCount2, byteOrder)
                            Else
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, AutoReadElementSize, RealElementCount2)
                            End If
                        Else
                            If (libplctagIsNew = 0) AndAlso byteOrder IsNot Nothing Then
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, AutoReadElementSize, AutoReadElementCount, byteOrder)
                            Else
                                tag2in = New LibplctagWrapper.Tag(prot, ipAddress, cpuType, address2poll, AutoReadElementSize, AutoReadElementCount)
                            End If
                        End If
                    End If

                    Dim tmpPI = New PollInfo With {.tag2 = tag2in, .Index = i, .bitIndex = bitIndex, .PlcAddress = stringValues(0), .DataType = DataType, .AutoReadElementCount = AutoReadElementCount, .AutoReadElementSize = AutoReadElementSize, .cpuType = cpuType, .ARPIDSuffix = AutoReadPIDSuffix}
                    AutoReadPIDSuffix = ""

                    AutoPollAddressList.Add(tmpPI)

                    AddressList(i).RadioBtn.Enabled = False
                    AddressList(i).PlcAddress.Enabled = False
                    AddressList(i).CheckBoxRead.Enabled = False
                    AddressList(i).CheckBoxWrite.Enabled = False
                    AddressList(i).ValuesToWrite.BackColor = Color.White
                    AddressList(i).ValuesToWrite.Text = ""
                    AddressList(i).ButtonSend.Enabled = False
                    AddressList(i).ButtonSend.BackColor = Color.Gainsboro
                    AddressList(i).LabelStatus.BackColor = Color.White
                End If
            Next

            If AutoPollAddressList.Count = 0 Then
                MessageBox.Show("No address selected for automatic reading!")
                chbAutoRead.Checked = False
                lblAutoReadStatusIndicator.BackColor = Color.White
                Exit Sub
            End If

            If AutoReadBckgndThread Is Nothing Then
                AutoReadBckgndThread = New Threading.Thread(AddressOf AutoRead) With {.IsBackground = True}
                AutoReadBckgndThread.Start()
            End If

            lblMessage.Text = "Processing Request"

            tbIPAddress.Enabled = False
            tbPath.Enabled = False
            cbCPUType.Enabled = False
            If cbCPUType.SelectedIndex = 0 Then
                tbProgramName.Enabled = False
            End If
            cbProtocol.Enabled = False
        Else
            cbPollInterval.Enabled = True
            tbIPAddress.Enabled = True
            tbPath.Enabled = True
            cbCPUType.Enabled = True
            If cbCPUType.SelectedIndex = 0 Then
                tbProgramName.Enabled = True
            End If
            cbProtocol.Enabled = True

            lblMessage.Text = ""
        End If
    End Sub

    Private Sub AutoRead()
        Dim AutoReadHoldRelease As New Threading.EventWaitHandle(False, Threading.EventResetMode.AutoReset)
        Dim holdTime As Integer
        Dim i As Integer

        While chbAutoRead.Checked
            Try
                If pollInterval > 0 Then
                    'Allow 5ms for each read
                    holdTime = Convert.ToInt32(Math.Floor(pollInterval / AutoPollAddressList.Count)) - 5
                End If

                i = 0

                While i < AutoPollAddressList.Count
                    If chbAutoRead.Checked Then AutoReadValues(AutoPollAddressList(i).tag2, AutoPollAddressList.Count, AutoPollAddressList(i).PlcAddress, AutoPollAddressList(i).DataType, AutoPollAddressList(i).bitIndex, AutoPollAddressList(i).AutoReadElementCount, AutoPollAddressList(i).AutoReadElementSize, AutoPollAddressList(i).Index, AutoPollAddressList(i).cpuType, chbBDOneZero.Checked, chbBDOnOff.Checked, AutoPollAddressList(i).ARPIDSuffix)

                    If holdTime > 0 Then AutoReadHoldRelease.WaitOne(holdTime)

                    i += 1
                End While
            Catch ex As Exception
                lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = ex.Message, MethodInvoker))
            End Try
        End While

        If AutoPollAddressList.Count > 0 Then
            For Each item In AutoPollAddressList
                AddressList(item.Index).RadioBtn.Invoke(DirectCast(Sub() AddressList(item.Index).RadioBtn.Enabled = True, MethodInvoker))
                AddressList(item.Index).PlcAddress.Invoke(DirectCast(Sub() AddressList(item.Index).PlcAddress.Enabled = True, MethodInvoker))
                AddressList(item.Index).CheckBoxRead.Invoke(DirectCast(Sub() AddressList(item.Index).CheckBoxRead.Enabled = True, MethodInvoker))
                AddressList(item.Index).CheckBoxWrite.Invoke(DirectCast(Sub() AddressList(item.Index).CheckBoxWrite.Enabled = True, MethodInvoker))
                AddressList(item.Index).ButtonSend.Invoke(DirectCast(Sub() AddressList(item.Index).ButtonSend.Enabled = True, MethodInvoker))
                AddressList(item.Index).ButtonSend.Invoke(DirectCast(Sub() AddressList(item.Index).ButtonSend.BackColor = Color.LightSteelBlue, MethodInvoker))
            Next

            AutoPollAddressList.Clear()
        End If

        If AutoReadMaster._tags.Count > 0 Then
            AutoReadMaster._tags.Clear()
        End If

        lblAutoReadStatusIndicator.Invoke(DirectCast(Sub() lblAutoReadStatusIndicator.BackColor = Color.White, MethodInvoker))
        AutoReadBckgndThread = Nothing
    End Sub

    Private Sub AutoReadValues(tag2 As LibplctagWrapper.Tag, tagCount As Integer, plcAddress As String, dataType As String, bitIndex As Integer, AutoReadElementCount As Integer, AutoReadElementSize As Integer, Index As Integer, cpuType As LibplctagWrapper.CpuType, BDOneZero As Boolean, BDOnOff As Boolean, ARPIDSuffix As String)
        If AutoReadMaster._tags.Count < tagCount Then
            If AutoReadMaster._tags.Count > 0 Then
                If Not AutoReadMaster._tags.ContainsKey(tag2.UniqueKey) Then
                    AutoReadMaster.AddTag(tag2, tbTimeout.Text)

                    While AutoReadMaster.GetStatus(tag2) = LibplctagWrapper.Libplctag.PLCTAG_STATUS_PENDING
                        Threading.Thread.Sleep(10)
                    End While

                    ' if the status is not ok, we have to handle the error
                    If AutoReadMaster.GetStatus(tag2) <> LibplctagWrapper.Libplctag.PLCTAG_STATUS_OK Then
                        AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.BackColor = Color.Salmon, MethodInvoker))
                        AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.Text = AutoReadMaster.DecodeError(AutoReadMaster.GetStatus(tag2)), MethodInvoker))
                        AutoReadMaster.RemoveTag(tag2)

                        'Set the AutoRead status label back color to Red = Failed
                        lblAutoReadStatusIndicator.Invoke(DirectCast(Sub() lblAutoReadStatusIndicator.BackColor = Color.Red, MethodInvoker))

                        Exit Sub
                    End If
                End If
            Else
                AutoReadMaster.AddTag(tag2, tbTimeout.Text)

                While AutoReadMaster.GetStatus(tag2) = LibplctagWrapper.Libplctag.PLCTAG_STATUS_PENDING
                    Threading.Thread.Sleep(10)
                End While

                ' if the status is not ok, we have to handle the error
                If AutoReadMaster.GetStatus(tag2) <> LibplctagWrapper.Libplctag.PLCTAG_STATUS_OK Then
                    AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.BackColor = Color.Salmon, MethodInvoker))
                    AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.Text = AutoReadMaster.DecodeError(AutoReadMaster.GetStatus(tag2)), MethodInvoker))
                    AutoReadMaster.RemoveTag(tag2)

                    'Set the AutoRead status label back color to Red = Failed
                    lblAutoReadStatusIndicator.Invoke(DirectCast(Sub() lblAutoReadStatusIndicator.BackColor = Color.Red, MethodInvoker))

                    Exit Sub
                End If
            End If
        End If

        Try
            AutoReadMaster.ReadTag(tag2, tbTimeout.Text)

            Dim elementShift2 As Integer

            Dim count As Integer

            If plcAddress.IndexOfAny("/") <> -1 Then
                count = 1
            Else
                count = AutoReadElementCount
            End If

            For i = 0 To count - 1
                If Not (plcAddress.IndexOfAny("/") <> -1 OrElse dataType = "BOOL" OrElse dataType = "BOOL Array" OrElse dataType = "Timer" OrElse dataType = "Counter" OrElse dataType = "Control") Then
                    Select Case dataType
                        Case "Int8", "SINT"
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                If chbSwapBytes.Checked Then
                                    Dim quot, remainder As Integer
                                    If i > 0 Then quot = Math.DivRem(i, 2, remainder)

                                    If i = count - 1 Then
                                        If remainder = 0 Then
                                            AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i + 1)
                                        Else
                                            AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i - 1)
                                        End If
                                    Else
                                        If remainder = 0 Then
                                            AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i + 1) & ", "
                                        Else
                                            AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i - 1) & ", "
                                        End If
                                    End If
                                Else
                                    If i = count - 1 Then
                                        AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i)
                                    Else
                                        AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i) & ", "
                                    End If
                                End If
                            Else
                                If i = count - 1 Then
                                    AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i * tag2.ElementSize)
                                Else
                                    AutoReadStrMessage &= AutoReadMaster.GetInt8Value(tag2, i * tag2.ElementSize) & ", "
                                End If
                            End If
                        Case "UInt8", "USINT"
                            If cpuType = LibplctagWrapper.CpuType.MODBUS Then
                                If chbSwapBytes.Checked Then
                                    Dim quot, remainder As Integer
                                    If i > 0 Then quot = Math.DivRem(i, 2, remainder)

                                    If i = count - 1 Then
                                        If remainder = 0 Then
                                            AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i + 1)
                                        Else
                                            AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i - 1)
                                        End If
                                    Else
                                        If remainder = 0 Then
                                            AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i + 1) & ", "
                                        Else
                                            AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i - 1) & ", "
                                        End If
                                    End If
                                Else
                                    If i = count - 1 Then
                                        AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i)
                                    Else
                                        AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i) & ", "
                                    End If
                                End If
                            Else
                                If i = count - 1 Then
                                    AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i * tag2.ElementSize)
                                Else
                                    AutoReadStrMessage &= AutoReadMaster.GetUint8Value(tag2, i * tag2.ElementSize) & ", "
                                End If
                            End If
                        Case "Int16", "INT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "UInt16", "UINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetUint16Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetUint16Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "Int32", "DINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "UInt32", "UDINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetUint32Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetUint32Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "Float32", "REAL"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetFloat32Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetFloat32Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "Int64", "LINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetInt64Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetInt64Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "UInt64", "ULINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetUint64Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetUint64Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "Float64", "LREAL"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetFloat64Value(tag2, i * tag2.ElementSize)
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetFloat64Value(tag2, i * tag2.ElementSize) & ", "
                            End If
                        Case "Int128", "QDINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetInt128Value(tag2, i * tag2.ElementSize).ToString
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetInt128Value(tag2, i * tag2.ElementSize).ToString & ", "
                            End If
                        Case "UInt128", "QUDINT"
                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadMaster.GetUint128Value(tag2, i * tag2.ElementSize).ToString
                            Else
                                AutoReadStrMessage &= AutoReadMaster.GetUint128Value(tag2, i * tag2.ElementSize).ToString & ", "
                            End If
                        Case "PID"
                            If ARPIDSuffix <> "" Then
                                Select Case ARPIDSuffix
                                    Case "EN"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 15), BDOneZero, BDOnOff)
                                    Case "DN"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 13), BDOneZero, BDOnOff)
                                    Case "PV"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 12), BDOneZero, BDOnOff)
                                    Case "SP"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 11), BDOneZero, BDOnOff)
                                    Case "LL"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 10), BDOneZero, BDOnOff)
                                    Case "UL"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 9), BDOneZero, BDOnOff)
                                    Case "DB"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 8), BDOneZero, BDOnOff)
                                    Case "DA"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 7), BDOneZero, BDOnOff)
                                    Case "TF"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 6), BDOneZero, BDOnOff)
                                    Case "SC"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 5), BDOneZero, BDOnOff)
                                    Case "RG"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 4), BDOneZero, BDOnOff)
                                    Case "OL"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 3), BDOneZero, BDOnOff)
                                    Case "CM"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 2), BDOneZero, BDOnOff)
                                    Case "AM"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 1), BDOneZero, BDOnOff)
                                    Case "TM"
                                        AutoReadStrMessage = AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 0), BDOneZero, BDOnOff)
                                    Case "SPS"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 2 * tag2.ElementSize)
                                    Case "KC"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 3 * tag2.ElementSize)
                                    Case "Ti"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 4 * tag2.ElementSize)
                                    Case "TD"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 5 * tag2.ElementSize)
                                    Case "MAXS"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 7 * tag2.ElementSize)
                                    Case "MINS"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 8 * tag2.ElementSize)
                                    Case "ZCD"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 9 * tag2.ElementSize)
                                    Case "CVH"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 11 * tag2.ElementSize)
                                    Case "CVL"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 12 * tag2.ElementSize)
                                    Case "LUT"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 13 * tag2.ElementSize)
                                    Case "SPV"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 14 * tag2.ElementSize)
                                    Case "CVP"
                                        AutoReadStrMessage = AutoReadMaster.GetInt16Value(tag2, 16 * tag2.ElementSize)
                                End Select
                            Else
                                For j = 0 To 22
                                    If j = 22 Then
                                        AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, j * tag2.ElementSize)
                                    Else
                                        AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, j * tag2.ElementSize) & ", "
                                    End If
                                Next
                            End If
                        Case "Custom String"
                            'Actual String Length from first 4 bytes
                            Dim actStrLgth As Integer = AutoReadMaster.GetInt32Value(tag2, i * tag2.ElementSize)

                            Dim valUShort() = New String(AutoReadCustomStringLength - 1) {}

                            For k = 0 To actStrLgth - 1
                                valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 4 + i * tag2.ElementSize).ToString
                            Next

                            If i = count - 1 Then
                                AutoReadStrMessage &= AutoReadConvertStringOfIntegersToString(valUShort)
                            Else
                                AutoReadStrMessage &= AutoReadConvertStringOfIntegersToString(valUShort) & ", "
                            End If
                        Case "String"
                            Dim valUShort() As String

                            If cpuType = 7 Then 'Modbus
                                valUShort = New String(AutoReadCustomStringLength - 1) {}
                                Dim valBytes = New Byte(AutoReadCustomStringLength - 1) {}

                                For k = 0 To AutoReadCustomStringLength - 1
                                    valBytes(k) = AutoReadMaster.GetUint8Value(tag2, k + i * tag2.ElementSize)
                                Next

                                valBytes = AutoReadSwapCheck(valBytes)

                                For k = 0 To AutoReadCustomStringLength - 1
                                    valUShort(k) = valBytes(k).ToString
                                Next
                            ElseIf cpuType = 2 Then 'Micro800
                                'String Length from first byte
                                Dim strLgth = AutoReadMaster.GetUint8Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 1 + i * tag2.ElementSize).ToString
                                Next
                            ElseIf cpuType = 0 Then 'ControlLogix
                                'String Length from first 4 bytes
                                Dim strLgth = AutoReadMaster.GetUint32Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 4 + i * tag2.ElementSize).ToString
                                Next
                            Else
                                'String Length from first 2 bytes
                                Dim strLgth = AutoReadMaster.GetUint16Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                Dim result As Integer
                                Dim quot = Math.DivRem(strLgth, 2, result)

                                If result = 0 Then
                                    valUShort = New String(strLgth - 1) {}
                                Else
                                    valUShort = New String(strLgth) {}
                                End If

                                'Reverse bytes
                                For k = 0 To valUShort.Length - 1 Step 2
                                    valUShort(k + 1) = AutoReadMaster.GetUint8Value(tag2, k + 2 + i * tag2.ElementSize).ToString
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 3 + i * tag2.ElementSize).ToString
                                Next
                            End If

                            Dim str = AutoReadConvertStringOfIntegersToString(valUShort)

                            If i = count - 1 Then
                                If str = "" Then
                                    AutoReadStrMessage &= "{}"
                                Else
                                    AutoReadStrMessage &= str
                                End If
                            Else
                                If str = "" Then
                                    AutoReadStrMessage &= "{}" & ", "
                                Else
                                    AutoReadStrMessage &= str & ", "
                                End If
                            End If
                    End Select
                Else
                    If plcAddress.IndexOfAny("/") <> -1 Then 'Bit or Character Reading
                        If dataType = "Custom String" Then
                            ' Read the whole string, get the substring and show its characters separated by a comma

                            'Actual String Length from first 4 bytes
                            Dim actStrLgth As Integer = AutoReadMaster.GetInt32Value(tag2, i * tag2.ElementSize)

                            Dim valUShort() = New String(AutoReadCustomStringLength - 1) {}

                            For k = 0 To actStrLgth - 1
                                valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 4 + i * tag2.ElementSize).ToString
                            Next

                            Dim finalString = AutoReadConvertStringOfIntegersToString(valUShort)

                            For m = bitIndex - 1 To bitIndex + AutoReadElementCount - 2
                                If m = bitIndex + AutoReadElementCount - 2 Then
                                    AutoReadStrMessage += finalString.Substring(m, 1)
                                Else
                                    AutoReadStrMessage += finalString.Substring(m, 1) + ", "
                                End If
                            Next
                        ElseIf dataType = "String" Then
                            Dim valUShort() As String = Nothing

                            ' Read the whole string, get the substring and show its characters separated by a comma

                            If cpuType = 7 Then 'Modbus
                                valUShort = New String(AutoReadCustomStringLength - 1) {}
                                Dim valBytes = New Byte(AutoReadCustomStringLength - 1) {}

                                For k = 0 To AutoReadCustomStringLength - 1
                                    valBytes(k) = AutoReadMaster.GetUint8Value(tag2, k + i * tag2.ElementSize)
                                Next

                                valBytes = AutoReadSwapCheck(valBytes)

                                For k = 0 To AutoReadCustomStringLength - 1
                                    valUShort(k) = valBytes(k).ToString
                                Next
                            ElseIf cpuType = 2 Then
                                'String Length from first byte
                                Dim strLgth = AutoReadMaster.GetUint8Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 1 + i * tag2.ElementSize).ToString
                                Next
                            ElseIf cpuType = 0 Then
                                'String Length from first 4 bytes
                                Dim strLgth = AutoReadMaster.GetUint32Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                For k = 0 To strLgth - 1
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 4 + i * tag2.ElementSize).ToString
                                Next
                            Else 'SLC Or PLC5
                                'String Length from first 2 bytes
                                Dim strLgth = AutoReadMaster.GetUint16Value(tag2, i * tag2.ElementSize)

                                valUShort = New String(strLgth - 1) {}

                                Dim result As Integer
                                Dim quot = Math.DivRem(strLgth, 2, result)

                                If result = 0 Then
                                    valUShort = New String(strLgth - 1) {}
                                Else
                                    valUShort = New String(strLgth) {}
                                End If

                                For k = 0 To valUShort.Length - 1 Step 2 'Reverse bytes
                                    valUShort(k + 1) = AutoReadMaster.GetUint8Value(tag2, k + 2 + i * tag2.ElementSize).ToString
                                    valUShort(k) = AutoReadMaster.GetUint8Value(tag2, k + 3 + i * tag2.ElementSize).ToString
                                Next
                            End If

                            Dim finalString = AutoReadConvertStringOfIntegersToString(valUShort)

                            If bitIndex > finalString.Length - 1 Then
                                AutoReadStrMessage += ""
                            Else
                                For m = bitIndex - 1 To bitIndex + AutoReadElementCount - 2
                                    If m = bitIndex + AutoReadElementCount - 2 Then
                                        AutoReadStrMessage += finalString.Substring(m, 1)
                                    Else
                                        AutoReadStrMessage += finalString.Substring(m, 1) + ", "
                                    End If
                                Next
                            End If
                        Else
                            If cpuType = 7 Then
                                Select Case dataType
                                    Case "Int8", "SINT", "UInt8", "USINT"
                                        For j = 0 To AutoReadElementCount - 1
                                            If j = AutoReadElementCount - 1 Then
                                                AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff)
                                            Else
                                                AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                            End If
                                        Next
                                    Case "Int16", "INT", "UInt16", "UINT"
                                        If chbSwapBytes.Checked Then
                                            Dim stringPart0 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 0), 2).PadLeft(8, "0"c))
                                            Dim stringPart1 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 1), 2).PadLeft(8, "0"c))
                                            Dim binaryString = stringPart1 & stringPart0

                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        End If
                                    Case "Int32", "DINT", "UInt32", "UDINT", "Float32", "REAL"
                                        If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                            Dim stringPart0 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 0), 2).PadLeft(8, "0"c))
                                            Dim stringPart1 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 1), 2).PadLeft(8, "0"c))
                                            Dim stringPart2 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 2), 2).PadLeft(8, "0"c))
                                            Dim stringPart3 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 3), 2).PadLeft(8, "0"c))
                                            Dim binaryString As String

                                            If chbSwapBytes.Checked Then
                                                If chbSwapWords.Checked Then
                                                    binaryString = stringPart3 & stringPart2 & stringPart1 & stringPart0
                                                Else
                                                    binaryString = stringPart1 & stringPart0 & stringPart3 & stringPart2
                                                End If
                                            Else
                                                binaryString = stringPart2 & stringPart3 & stringPart0 & stringPart1
                                            End If

                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        End If
                                    Case "Int64", "LINT", "UInt64", "ULINT", "Float64", "LREAL"
                                        If chbSwapBytes.Checked OrElse chbSwapWords.Checked Then
                                            Dim stringPart0 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 0), 2).PadLeft(8, "0"c))
                                            Dim stringPart1 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 1), 2).PadLeft(8, "0"c))
                                            Dim stringPart2 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 2), 2).PadLeft(8, "0"c))
                                            Dim stringPart3 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 3), 2).PadLeft(8, "0"c))
                                            Dim stringPart4 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 4), 2).PadLeft(8, "0"c))
                                            Dim stringPart5 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 5), 2).PadLeft(8, "0"c))
                                            Dim stringPart6 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 6), 2).PadLeft(8, "0"c))
                                            Dim stringPart7 = StrReverse(Convert.ToString(AutoReadMaster.GetUint8Value(tag2, 7), 2).PadLeft(8, "0"c))
                                            Dim binaryString As String

                                            If chbSwapBytes.Checked Then
                                                If chbSwapWords.Checked Then
                                                    binaryString = stringPart7 & stringPart6 & stringPart5 & stringPart4 & stringPart3 & stringPart2 & stringPart1 & stringPart0
                                                Else
                                                    binaryString = stringPart1 & stringPart0 & stringPart3 & stringPart2 & stringPart5 & stringPart4 & stringPart7 & stringPart6
                                                End If
                                            Else
                                                binaryString = stringPart6 & stringPart7 & stringPart4 & stringPart5 & stringPart2 & stringPart3 & stringPart0 & stringPart1
                                            End If

                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadConvertToBoolean(binaryString(bitIndex + j)), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To AutoReadElementCount - 1
                                                If j = AutoReadElementCount - 1 Then
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                                End If
                                            Next
                                        End If
                                End Select
                            Else
                                For j = 0 To AutoReadElementCount - 1
                                    If j = AutoReadElementCount - 1 Then
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff)
                                    Else
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + j), BDOneZero, BDOnOff) & ", "
                                    End If
                                Next
                            End If
                        End If
                    Else
                        Select Case dataType
                            Case "BOOL"
                                If cpuType = 0 OrElse cpuType = 2 Then
                                    AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, 0), BDOneZero, BDOnOff)
                                ElseIf cpuType = 7 Then ' Modbus
                                    If i = count - 1 Then
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + i + 1), BDOneZero, BDOnOff)
                                    Else
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + i + 1), BDOneZero, BDOnOff) & ", "
                                    End If
                                Else
                                    If i = count - 1 Then
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + i), BDOneZero, BDOnOff)
                                    Else
                                        AutoReadStrMessage &= AutoReadBooleanDisplay(AutoReadMaster.GetBitValue(tag2, bitIndex + i), BDOneZero, BDOnOff) & ", "
                                    End If
                                End If
                            Case "BOOL Array"
                                Dim remainder1 As Integer
                                Dim startElement1 As Integer = Math.DivRem(bitIndex, tag2.ElementSize * 8, remainder1)

                                If bitIndex + i > 0 Then
                                    Dim remainder2 As Integer
                                    Dim quotient As Integer = Math.DivRem(bitIndex + i, tag2.ElementSize * 8, remainder2)
                                    If remainder2 = 0 Then
                                        elementShift2 += 1
                                    End If
                                End If

                                If i = count - 1 Then
                                    AutoReadStrMessage &= AutoReadExtractInt32Bit(AutoReadMaster.GetInt32Value(tag2, (startElement1 + elementShift2) * tag2.ElementSize), bitIndex - ((startElement1 + elementShift2) * tag2.ElementSize * 8) + i, BDOneZero, BDOnOff)
                                Else
                                    AutoReadStrMessage &= AutoReadExtractInt32Bit(AutoReadMaster.GetInt32Value(tag2, (startElement1 + elementShift2) * tag2.ElementSize), bitIndex - ((startElement1 + elementShift2) * tag2.ElementSize * 8) + i, BDOneZero, BDOnOff) & ", "
                                End If
                            Case "Timer"
                                If cpuType = 0 OrElse cpuType = 2 Then
                                    If i = count - 1 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4)
                                            Else
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        For j = 0 To 2
                                            AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                        Next
                                    End If
                                Else
                                    If plcAddress.IndexOfAny(".") <> -1 Then
                                        Dim tempVal = AutoReadMaster.GetInt16Value(tag2, i * tag2.ElementSize)

                                        Select Case plcAddress.Substring(plcAddress.IndexOf(".") + 1)
                                            Case "EN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "TT"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "DN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "PRE"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2) & ", "
                                                End If
                                            Case "ACC"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4) & ", "
                                                End If
                                        End Select
                                    Else
                                        If i = count - 1 Then
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To 2
                                                AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                            Next
                                        End If
                                    End If
                                End If
                            Case "Counter"
                                If cpuType = 0 OrElse cpuType = 2 Then
                                    If i = count - 1 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4)
                                            Else
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        For j = 0 To 2
                                            AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                        Next
                                    End If
                                Else
                                    If plcAddress.IndexOfAny(".") <> -1 Then
                                        Dim tempVal = AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize)

                                        Select Case plcAddress.Substring(plcAddress.IndexOf(".") + 1)
                                            Case "CU"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "CD"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "DN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "OV"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "UN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "UA"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "PRE"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2) & ", "
                                                End If
                                            Case "ACC"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4) & ", "
                                                End If
                                        End Select
                                    Else
                                        If i = count - 1 Then
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To 2
                                                AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                            Next
                                        End If
                                    End If
                                End If
                            Case "Control"
                                If cpuType = 0 OrElse cpuType = 2 Then
                                    If i = count - 1 Then
                                        For j = 0 To 2
                                            If j = 2 Then
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4)
                                            Else
                                                AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                            End If
                                        Next
                                    Else
                                        For j = 0 To 2
                                            AutoReadStrMessage &= AutoReadMaster.GetInt32Value(tag2, i * AutoReadElementSize + j * 4) & ", "
                                        Next
                                    End If
                                Else
                                    If plcAddress.IndexOfAny(".") <> -1 Then
                                        Dim tempVal = AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize)

                                        Select Case plcAddress.Substring(plcAddress.IndexOf(".") + 1)
                                            Case "EN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 15, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "EU"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 14, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "DN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 13, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "EM"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 12, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "ER"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 11, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "UL"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 10, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "IN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 9, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 9, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "FD"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 8, BDOneZero, BDOnOff)
                                                Else
                                                    AutoReadStrMessage &= AutoReadExtractInt16Bit(tempVal, 8, BDOneZero, BDOnOff) & ", "
                                                End If
                                            Case "LEN"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 2) & ", "
                                                End If
                                            Case "POS"
                                                If i = count - 1 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + 4) & ", "
                                                End If
                                        End Select
                                    Else
                                        If i = count - 1 Then
                                            For j = 0 To 2
                                                If j = 2 Then
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2)
                                                Else
                                                    AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                                End If
                                            Next
                                        Else
                                            For j = 0 To 2
                                                AutoReadStrMessage &= AutoReadMaster.GetInt16Value(tag2, i * AutoReadElementSize + j * 2) & ", "
                                            Next
                                        End If
                                    End If
                                End If
                        End Select
                    End If
                End If
            Next

            AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.BackColor = Color.White, MethodInvoker))
            AddressList(Index).ValuesToWrite.Invoke(DirectCast(Sub() AddressList(Index).ValuesToWrite.Text = AutoReadStrMessage, MethodInvoker))
            AutoReadStrMessage = ""
        Catch ex As Exception
            lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = ex.Message, MethodInvoker))
            AutoReadStrMessage = ""

            'Set the AutoRead status label back color to Red = Failed
            lblAutoReadStatusIndicator.Invoke(DirectCast(Sub() lblAutoReadStatusIndicator.BackColor = Color.Red, MethodInvoker))

            Exit Sub
        End Try

        AutoReadStrMessage = ""

        lblMessage.Invoke(DirectCast(Sub() lblMessage.Text = "Comms Okay", MethodInvoker))

        'Set the AutoRead status label back color to LimeGreen = Success
        lblAutoReadStatusIndicator.Invoke(DirectCast(Sub() lblAutoReadStatusIndicator.BackColor = Color.LimeGreen, MethodInvoker))
    End Sub

#End Region

#Region "Get Tags"

    Private Sub ButtonGetTags_Click(sender As Object, e As MouseEventArgs) Handles btnGetTags.Click
        Try
            Dim cpuType = LibplctagWrapper.CpuType.ControlLogix
            Dim prot As String = "ab_eip"
            Dim ipAddress As String = tbIPAddress.Text
            Dim path As String = tbPath.Text.Replace(" ", "")
            Dim timeout As Integer = tbTimeout.Text
            Dim name1 As String = "@tags" 'Controller tags
            Dim name2 As String = ""

            If tbProgramName.Text <> "" Then
                name2 = "Program:" & tbProgramName.Text & ".@tags" 'Program tags
            End If

            Dim tagsList = New List(Of String)()

            cbTags.Items.Clear()

            Dim tagC = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, name1)

            Master.AddTag(tagC)

            Dim count As Integer

            While Master.GetStatus(tagC) = LibplctagWrapper.Libplctag.PLCTAG_STATUS_PENDING
                count += 1

                If count > 500 Then
                    MessageBox.Show("PLC is not responding! Try again.")
                    Master.RemoveTag(tagC)
                    Exit Sub
                End If

                Threading.Thread.Sleep(10)
            End While

            ' if the status is not ok, we have to handle the error
            If Master.GetStatus(tagC) <> LibplctagWrapper.Libplctag.PLCTAG_STATUS_OK Then
                lblMessage.Text = "Error - " & Master.DecodeError(Master.GetStatus(tagC))
                Master.RemoveTag(tagC)
                Exit Sub
            End If

            cbTags.Items.Add("***  Controller Tags List  ***")

            Master.ReadTag(tagC, timeout)

            Dim tagSize = Master.GetSize(tagC)
            Dim offset As Integer = 0

            While offset < tagSize
                Dim tagId = Master.GetUint32Value(tagC, offset)
                Dim tagType = Master.GetUint16Value(tagC, offset + 4)
                Dim tagLength = Master.GetUint16Value(tagC, offset + 6)

                Dim systemBit As Boolean = ExtractInt32Bit(tagType, 12, False, False)

                If Not systemBit Then
                    Dim IsStructure As Boolean = ExtractInt32Bit(tagType, 15, False, False)

                    Dim x = Master.GetUint32Value(tagC, offset + 8)
                    Dim y = Master.GetUint32Value(tagC, offset + 12)
                    Dim z = Master.GetUint32Value(tagC, offset + 16)

                    Dim dimensions As String = ""

                    If x <> 0 AndAlso y <> 0 AndAlso z <> 0 Then
                        dimensions = "[" & x & ", " & y & ", " & z & "]"
                    ElseIf x <> 0 AndAlso y <> 0 Then
                        dimensions = "[" & x & ", " & y & "]"
                    ElseIf x <> 0 Then
                        If tagType = 8403 Then
                            dimensions = "[" & x * 32 & "]"
                        Else
                            dimensions = "[" & x & "]"
                        End If
                    End If

                    offset += 20

                    Dim tagNameLength As Integer = Master.GetUint16Value(tagC, offset)
                    Dim tagNameBytes(tagNameLength - 1) As Byte

                    offset += 2

                    For i = 0 To tagNameLength - 1
                        tagNameBytes(i) = Master.GetUint8Value(tagC, offset + i)
                    Next

                    Dim tagName = System.Text.Encoding.ASCII.GetString(tagNameBytes)

                    'Uncomment the following If section if you want to avoid displaying local modules tags
                    'If Not tagName.Contains(":") Then
                    'The following line would allow displaying more info than just tag name and its dimensions
                    'tagsList.Add(tagName & dimensions & " ; Type = " & tagType & " ; IsStructure = " & IsStructure & " ; Length = " & tagLength & " Bytes ; Id = " & tagId)
                    tagsList.Add(tagName & dimensions)
                    'End If

                    offset += tagNameLength
                Else
                    offset += 20
                    Dim tagNameLength As Integer = Master.GetUint16Value(tagC, offset)
                    offset += 2 + tagNameLength
                End If
            End While

            Dim tagsArray = tagsList.ToArray
            Array.Sort(tagsArray)
            tagsList.Clear()

            For i = 0 To tagsArray.Length - 1
                cbTags.Items.Add(tagsArray(i))
            Next

            cbTags.Items.Add("")

            Master.RemoveTag(tagC)

            If name2 = "" Then
                cbTags.Items.Add("***  Program Tags List (no program specified)  ***")
            Else
                Dim tagP = New LibplctagWrapper.Tag(prot, ipAddress, path, cpuType, name2)

                Master.AddTag(tagP)

                count = 0

                While Master.GetStatus(tagP) = LibplctagWrapper.Libplctag.PLCTAG_STATUS_PENDING
                    count += 1

                    If count > 500 Then
                        MessageBox.Show("PLC is not responding! Try again.")
                        Master.RemoveTag(tagP)
                        Exit Sub
                    End If

                    Threading.Thread.Sleep(10)
                End While

                ' if the status is not ok, we have to handle the error
                If Master.GetStatus(tagP) <> LibplctagWrapper.Libplctag.PLCTAG_STATUS_OK Then
                    lblMessage.Text = "Error - " & Master.DecodeError(Master.GetStatus(tagP))
                    Master.RemoveTag(tagP)
                    Exit Sub
                End If

                cbTags.Items.Add("***  Program Tags List (" & tbProgramName.Text & ")  ***")

                Master.ReadTag(tagP, timeout)

                tagSize = Master.GetSize(tagP)
                offset = 0

                While offset < tagSize
                    Dim tagId = Master.GetUint32Value(tagP, offset)
                    Dim tagType = Master.GetUint16Value(tagP, offset + 4)
                    Dim tagLength = Master.GetUint16Value(tagP, offset + 6)

                    Dim systemBit As Boolean = ExtractInt32Bit(tagType, 12, False, False)

                    If Not systemBit Then
                        Dim IsStructure As Boolean = ExtractInt32Bit(tagType, 15, False, False)

                        Dim x = Master.GetUint32Value(tagP, offset + 8)
                        Dim y = Master.GetUint32Value(tagP, offset + 12)
                        Dim z = Master.GetUint32Value(tagP, offset + 16)

                        Dim dimensions As String = ""

                        If x <> 0 AndAlso y <> 0 AndAlso z <> 0 Then
                            dimensions = "[" & x & ", " & y & ", " & z & "]"
                        ElseIf x <> 0 AndAlso y <> 0 Then
                            dimensions = "[" & x & ", " & y & "]"
                        ElseIf x <> 0 Then
                            If tagType = 8403 Then
                                dimensions = "[" & x * 32 & "]"
                            Else
                                dimensions = "[" & x & "]"
                            End If
                        End If

                        offset += 20

                        Dim tagNameLength As Integer = Master.GetUint16Value(tagP, offset)
                        Dim tagNameBytes(tagNameLength - 1) As Byte

                        offset += 2

                        For i = 0 To tagNameLength - 1
                            tagNameBytes(i) = Master.GetUint8Value(tagP, offset + i)
                        Next

                        Dim tagName = System.Text.Encoding.ASCII.GetString(tagNameBytes)

                        'tagsList.Add("Program:" & tbProgramName.Text & "." & tagName & dimensions & " ; Type = " & tagType & " ; IsStructure = " & IsStructure & " ; Length = " & tagLength & " Bytes ; Id = " & tagId)
                        tagsList.Add("Program:" & tbProgramName.Text & "." & tagName & dimensions)

                        offset += tagNameLength
                    Else
                        offset += 20
                        Dim tagNameLength As Integer = Master.GetUint16Value(tagP, offset)
                        offset += 2 + tagNameLength
                    End If
                End While

                tagsArray = tagsList.ToArray
                Array.Sort(tagsArray)

                For i = 0 To tagsArray.Length - 1
                    cbTags.Items.Add(tagsArray(i))
                Next

                Master.RemoveTag(tagP)
            End If

            cbTags.SelectedIndex = 0
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

#End Region

#Region "Functions"

    Private Function CheckWriteValues(Index As Integer) As Boolean
        If AddressList(Index).CheckBoxWrite.Checked Then
            'Split tag string based on semicolon delimiter (PLC Address, Data Type, Element Count)
            Dim stringValues = AddressList(Index).PlcAddress.Text.Split(New Char() {";"c})

            For j = 0 To stringValues.Length - 1
                stringValues(j) = stringValues(j).Trim
            Next

            Dim bitIndex As Integer = -1

            If stringValues(0).IndexOfAny("/") <> -1 Then bitIndex = CInt(stringValues(0).Substring(stringValues(0).IndexOf("/") + 1))

            'Split ValuesToWrite string based on comma delimiter
            Dim vals = AddressList(Index).ValuesToWrite.Text.Split(New Char() {","c})

            For j = 0 To vals.Length - 1
                vals(j) = vals(j).Trim
            Next

            If String.IsNullOrWhiteSpace(AddressList(Index).ValuesToWrite.Text) AndAlso Not (stringValues(1) = "String" OrElse stringValues(1) = "Custom String") Then
                MessageBox.Show("No values to write are provided!")
                Return False
            Else
                If vals.Length > 1 AndAlso vals.Length <> elementCount Then
                    MessageBox.Show("The number of provided values to write does not match the element count!")
                    Return False
                Else
                    If (bitIndex > -1 AndAlso Not (stringValues(1) = "String" OrElse stringValues(1) = "Custom String")) OrElse stringValues(1) = "BOOL" OrElse stringValues(1) = "BOOL Array" Then
                        For i = 0 To vals.Length - 1
                            If Not (vals(i) = "0" OrElse vals(i) = "1" OrElse vals(i) = "True" OrElse vals(i) = "true" OrElse vals(i) = "False" OrElse vals(i) = "false") Then
                                MessageBox.Show("One of the values to write is not of Boolean type!")
                                Return False
                            End If
                        Next
                    Else
                        If stringValues(1) = "String" OrElse stringValues(1) = "Custom String" Then
                            If stringValues(0).IndexOfAny("/") <> -1 Then ' Character Writing
                                If bitIndex > 0 Then
                                    For i = 0 To vals.Length - 1
                                        If vals(i).Length > 1 Then
                                            MessageBox.Show("One of the values to write is not a single character!")
                                            Return False
                                        End If
                                    Next
                                Else
                                    MessageBox.Show("Character indexes start at 1!")
                                    Return False
                                End If
                            Else ' String Writing
                                If elementCount > 2 AndAlso cpuTypeIndex > 1 AndAlso cpuTypeIndex < 7 Then
                                    MessageBox.Show("Maximum 2 string values can be written in a single transaction!")
                                    Return False
                                Else
                                    For i = 0 To vals.Length - 1
                                        Dim txt As String = ""
                                        cbCPUType.Invoke(DirectCast(Sub() txt = cbCPUType.SelectedItem.ToString, MethodInvoker))
                                        If (txt = "LGX" AndAlso vals(i).Length > 84) OrElse (txt <> "LGX" AndAlso vals(i).Length > 82) Then
                                            MessageBox.Show("String length exceeds the element size!")
                                            Return False
                                        End If
                                    Next
                                End If
                            End If
                        Else
                            If stringValues(1) = "Int8" OrElse stringValues(1) = "SINT" Then
                                Dim dummy As SByte

                                For i = 0 To vals.Length - 1
                                    If Not SByte.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of SByte (Int8) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "UInt8" OrElse stringValues(1) = "USINT" Then
                                Dim dummy As Byte

                                For i = 0 To vals.Length - 1
                                    If Not Byte.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Byte (UInt8) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Int16" OrElse stringValues(1) = "INT" Then
                                Dim dummy As Short

                                For i = 0 To vals.Length - 1
                                    If Not Short.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Short (Int16) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "UInt16" OrElse stringValues(1) = "UINT" Then
                                Dim dummy As UShort

                                For i = 0 To vals.Length - 1
                                    If Not UShort.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of UShort (UInt16) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Int32" OrElse stringValues(1) = "DINT" Then
                                Dim dummy As Integer

                                For i = 0 To vals.Length - 1
                                    If Not Integer.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Integer (Int32) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "UInt32" OrElse stringValues(1) = "UDINT" Then
                                Dim dummy As UInteger

                                For i = 0 To vals.Length - 1
                                    If Not UInteger.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of UInteger (UInt32) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Float32" OrElse stringValues(1) = "REAL" Then
                                Dim dummy As Single

                                For i = 0 To vals.Length - 1
                                    If Not Single.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Single (Float32) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Int64" OrElse stringValues(1) = "LINT" Then
                                Dim dummy As Long

                                For i = 0 To vals.Length - 1
                                    If Not Long.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Long (Int64) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "UInt64" OrElse stringValues(1) = "ULINT" Then
                                Dim dummy As ULong

                                For i = 0 To vals.Length - 1
                                    If Not ULong.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of ULong (UInt64) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Float64" OrElse stringValues(1) = "LREAL" Then
                                Dim dummy As Double

                                For i = 0 To vals.Length - 1
                                    If Not Double.TryParse(vals(i), dummy) Then
                                        MessageBox.Show("Provided values to write are not of Double (Float64) type!")
                                        Return False
                                    End If
                                Next
                            ElseIf stringValues(1) = "Int128" OrElse stringValues(1) = "QDINT" Then
                                For i = 0 To vals.Length - 1
                                    Try
                                        'If negative value then use its absolute value
                                        If (Convert.ToInt32(vals(i)(0)) = 8722 OrElse Convert.ToInt32(vals(i)(0)) = 45) AndAlso Not (Convert.ToInt32(vals(i)(1)) = 8722 OrElse Convert.ToInt32(vals(i)(1)) = 45) Then
                                            If BigInteger.Parse(vals(i).Substring(1)) > BigInteger.Parse("170141183460469231731687303715884105728") Then
                                                MessageBox.Show("Provided values to write are not of Int128 type!")
                                                Return False
                                            End If
                                        Else
                                            If BigInteger.Parse(vals(i)) > BigInteger.Parse("170141183460469231731687303715884105727") Then
                                                MessageBox.Show("Provided values to write are not of Int128 type!")
                                                Return False
                                            End If
                                        End If
                                    Catch ex As Exception
                                        MessageBox.Show(ex.Message)
                                        Return False
                                    End Try
                                Next
                            ElseIf stringValues(1) = "UInt128" OrElse stringValues(1) = "QUDINT" Then
                                Dim upperLimit As BigInteger = BigInteger.Parse("340282366920938463463374607431768211455")
                                Dim dummy As BigInteger = 0

                                For i = 0 To vals.Length - 1
                                    Try
                                        If Convert.ToInt32(vals(i)(0)) = 8722 OrElse Convert.ToInt32(vals(i)(0)) = 45 Then
                                            MessageBox.Show("Provided values to write are not of UInt128 type!")
                                            Return False
                                        Else
                                            If BigInteger.TryParse(vals(i), dummy) Then
                                                If dummy < 0 OrElse dummy > upperLimit Then
                                                    MessageBox.Show("Provided values to write are not of UInt128 type!")
                                                    Return False
                                                End If
                                            End If
                                        End If
                                    Catch ex As Exception
                                        MessageBox.Show(ex.Message)
                                        Return False
                                    End Try
                                Next
                            End If
                        End If
                    End If
                End If
            End If
        End If

        Return True
    End Function

    Private Function ConvertStringToStringOfUShorts(str As String) As UShort()
        '* Convert string to an array of bytes
        Dim ByteArray() As Byte = System.Text.Encoding.Default.GetBytes(str)

        '* Convert each byte to ushort
        Dim ushorts(ByteArray.Length - 1) As UShort

        For i = 0 To ByteArray.Length - 1
            ushorts(i) = ByteArray(i)
        Next

        '* Return the ushort array
        Return ushorts
    End Function

    Private Function ConvertStringOfIntegersToString(ints() As String) As String
        '* Convert integer values to strings and then to an array of bytes
        Dim ByteArray(ints.Length - 1) As Byte

        For i = 0 To ints.Length - 1
            If CInt(ints(i)) = 0 Then
                ByteArray(i) = AscW(" "c)
            Else
                ByteArray(i) = CByte(ints(i))
            End If
        Next

        '* Convert the array of bytes to a string
        Dim result As String = System.Text.Encoding.Default.GetString(ByteArray)

        'Return the string
        Return result.Trim
    End Function

    Private Function AutoReadConvertStringOfIntegersToString(ints() As String) As String
        '* Convert integer values to strings and then to an array of bytes
        Dim ByteArray(ints.Length - 1) As Byte

        For i = 0 To ints.Length - 1
            If CInt(ints(i)) = 0 Then
                ByteArray(i) = AscW(" "c)
            Else
                ByteArray(i) = CByte(ints(i))
            End If
        Next

        '* Convert the array of bytes to a string
        Dim result As String = System.Text.Encoding.Default.GetString(ByteArray)

        'Return the string
        Return result.Trim
    End Function

    Private Function ExtractInt32Bit(ReadValue As Integer, BitToReturn As Integer, BDOneZero As Boolean, BDOnOff As Boolean) As String
        SyncLock m_Lock
            Dim bitString = Convert.ToString(ReadValue, 2).PadLeft(32, "0"c)

            If bitString(31 - BitToReturn) = "0"c Then
                If BDOneZero Then
                    Return "0"
                ElseIf BDOnOff Then
                    Return "Off"
                Else
                    Return "False"
                End If
            Else
                If BDOneZero Then
                    Return "1"
                ElseIf BDOnOff Then
                    Return "On"
                Else
                    Return "True"
                End If
            End If
        End SyncLock
    End Function

    Private Function AutoReadExtractInt32Bit(ReadValue As Integer, BitToReturn As Integer, BDOneZero As Boolean, BDOnOff As Boolean) As String
        SyncLock m_Lock
            Dim bitString = Convert.ToString(ReadValue, 2).PadLeft(32, "0"c)

            If bitString(31 - BitToReturn) = "0"c Then
                If BDOneZero Then
                    Return "0"
                ElseIf BDOnOff Then
                    Return "Off"
                Else
                    Return "False"
                End If
            Else
                If BDOneZero Then
                    Return "1"
                ElseIf BDOnOff Then
                    Return "On"
                Else
                    Return "True"
                End If
            End If
        End SyncLock
    End Function

    Private Function ExtractInt16Bit(ReadValue As Short, BitToReturn As Integer, BDOneZero As Boolean, BDOnOff As Boolean) As String
        SyncLock m_Lock
            Dim bitString = Convert.ToString(ReadValue, 2).PadLeft(16, "0"c)

            If bitString(15 - BitToReturn) = "0"c Then
                If BDOneZero Then
                    Return "0"
                ElseIf BDOnOff Then
                    Return "Off"
                Else
                    Return "False"
                End If
            Else
                If BDOneZero Then
                    Return "1"
                ElseIf BDOnOff Then
                    Return "On"
                Else
                    Return "True"
                End If
            End If
        End SyncLock
    End Function

    Private Function AutoReadExtractInt16Bit(ReadValue As Short, BitToReturn As Integer, BDOneZero As Boolean, BDOnOff As Boolean) As String
        SyncLock m_Lock
            Dim bitString = Convert.ToString(ReadValue, 2).PadLeft(16, "0"c)

            If bitString(15 - BitToReturn) = "0"c Then
                If BDOneZero Then
                    Return "0"
                ElseIf BDOnOff Then
                    Return "Off"
                Else
                    Return "False"
                End If
            Else
                If BDOneZero Then
                    Return "1"
                ElseIf BDOnOff Then
                    Return "On"
                Else
                    Return "True"
                End If
            End If
        End SyncLock
    End Function

    Private Function ChangeInt32Bit(ReadValue As Integer, BitToModify As Integer, BitValueToWrite As String) As Integer
        SyncLock m_Lock
            Dim bits = Convert.ToString(ReadValue, 2).PadLeft(32, "0"c)

            If BitValueToWrite = "0" OrElse BitValueToWrite = "False" OrElse BitValueToWrite = "false" Then
                BitValueToWrite = 0
            Else
                BitValueToWrite = 1
            End If

            Dim retValueBinary = bits.Substring(0, 31 - BitToModify) & BitValueToWrite & bits.Substring(31 - BitToModify + 1)

            Return Convert.ToInt32(retValueBinary, 2)
        End SyncLock
    End Function

    Private Function BooleanDisplay(boolValue As Boolean, BDOneZero As Boolean, BDOnOff As Boolean) As String
        If boolValue Then
            If BDOneZero Then
                Return "1"
            ElseIf BDOnOff Then
                Return "On"
            Else
                Return "True"
            End If
        Else
            If BDOneZero Then
                Return "0"
            ElseIf BDOnOff Then
                Return "Off"
            Else
                Return "False"
            End If
        End If
    End Function

    Private Function AutoReadBooleanDisplay(boolValue As Boolean, BDOneZero As Boolean, BDOnOff As Boolean) As String
        If boolValue Then
            If BDOneZero Then
                Return "1"
            ElseIf BDOnOff Then
                Return "On"
            Else
                Return "True"
            End If
        Else
            If BDOneZero Then
                Return "0"
            ElseIf BDOnOff Then
                Return "Off"
            Else
                Return "False"
            End If
        End If
    End Function

    Private Function SwapCheck(bytes As Byte()) As Byte()
        If Not chbSwapWords.Checked AndAlso bytes.Length > 3 Then
            For i = 0 To bytes.Length / 2 - 1
                Dim tempByte = bytes(i)
                bytes(i) = bytes(bytes.Length / 2 + i)
                bytes(bytes.Length / 2 + i) = tempByte
            Next
        End If

        If Not chbSwapBytes.Checked Then
            For i = 0 To bytes.Length - 1 Step 2
                Dim tempByte = bytes(i)
                bytes(i) = bytes(i + 1)
                bytes(i + 1) = tempByte
            Next
        End If

        Return bytes
    End Function

    Private Function SwapCheckUshort(bytes As UShort()) As UShort()
        If Not chbSwapWords.Checked AndAlso bytes.Length > 3 Then
            For i = 0 To bytes.Length / 2 - 1
                Dim tempByte = bytes(i)
                bytes(i) = bytes(bytes.Length / 2 + i)
                bytes(bytes.Length / 2 + i) = tempByte
            Next
        End If

        If Not chbSwapBytes.Checked Then
            For i = 0 To bytes.Length - 1 Step 2
                Dim tempByte = bytes(i)
                bytes(i) = bytes(i + 1)
                bytes(i + 1) = tempByte
            Next
        End If

        Return bytes
    End Function

    Private Function AutoReadSwapCheck(bytes As Byte()) As Byte()
        If Not chbSwapWords.Checked AndAlso bytes.Length > 3 Then
            For i = 0 To bytes.Length / 2 - 1
                Dim tempByte = bytes(i)
                bytes(i) = bytes(bytes.Length / 2 + i)
                bytes(bytes.Length / 2 + i) = tempByte
            Next
        End If

        If Not chbSwapBytes.Checked Then
            For i = 0 To bytes.Length - 1 Step 2
                Dim tempByte = bytes(i)
                bytes(i) = bytes(i + 1)
                bytes(i + 1) = tempByte
            Next
        End If

        Return bytes
    End Function

    Private Function AutoReadSwapCheckUshort(bytes As UShort()) As UShort()
        If Not chbSwapWords.Checked AndAlso bytes.Length > 3 Then
            For i = 0 To bytes.Length / 2 - 1
                Dim tempByte = bytes(i)
                bytes(i) = bytes(bytes.Length / 2 + i)
                bytes(bytes.Length / 2 + i) = tempByte
            Next
        End If

        If Not chbSwapBytes.Checked Then
            For i = 0 To bytes.Length - 1 Step 2
                Dim tempByte = bytes(i)
                bytes(i) = bytes(i + 1)
                bytes(i + 1) = tempByte
            Next
        End If

        Return bytes
    End Function

    Private Function BigInteger2BinaryString(BigIntegerBytes As Byte()) As String
        Dim bitString As String = ""

        For i = 0 To BigIntegerBytes.Length - 1
            bitString &= Convert.ToString(BigIntegerBytes(i)).PadLeft(8, "0"c)
        Next

        Return bitString
    End Function

    Private Function BitConverterInt128(ByVal binaryString As String) As BigInteger
        SyncLock m_Lock
            Dim Int128 As BigInteger = 0
            Dim base2 As BigInteger = 2
            Dim biMin As BigInteger = -2 ^ (binaryString.Length - 1)

            For i = 0 To binaryString.Length - 2
                If binaryString(binaryString.Length - 1 - i) = "1"c Then
                    Int128 += BigInteger.Pow(base2, i)
                End If
            Next

            If binaryString(0) = "0"c Then
                Return Int128
            Else
                Return BigInteger.Add(biMin, Int128)
            End If
        End SyncLock
    End Function

    Private Function BitConverterUInt128(ByVal binaryString As String) As BigInteger
        SyncLock m_Lock
            Dim UInt128 As BigInteger = 0
            Dim base2 As BigInteger = 2

            For i = 0 To binaryString.Length - 1
                If binaryString(binaryString.Length - 1 - i) = "1"c Then
                    UInt128 += BigInteger.Pow(base2, i)
                End If
            Next

            Return UInt128
        End SyncLock
    End Function

    Private Function ConvertToBoolean(str As String) As Boolean
        Return str = "1"
    End Function

    Private Function AutoReadConvertToBoolean(str As String) As Boolean
        Return str = "1"
    End Function

#End Region

#Region "ToolTips"

    Private Sub LabelStatus_MouseHover(sender As Object, e As EventArgs) Handles lblStatus.MouseHover
        AllToolTip.SetToolTip(sender, "WHITE = Inactive" & Environment.NewLine & "YELLOW = Processing" & Environment.NewLine & "GREEN = Success" & Environment.NewLine & "RED = Failed")
    End Sub

    Private Sub LabelAutoReadStatus_MouseHover(sender As Object, e As EventArgs) Handles lblAutoReadStatus.MouseHover
        AllToolTip.SetToolTip(sender, "WHITE = Inactive" & Environment.NewLine & "GREEN = Success" & Environment.NewLine & "RED = Failed")
    End Sub

    Private Sub LabelVTW_MouseHover(sender As Object, e As EventArgs) Handles lblVTW.MouseHover
        AllToolTip.SetToolTip(sender, "Write - Either single or the exact number of comma separated values required if element count > 1." & Environment.NewLine & "Read - Received values will be displayed as Read-Only.")
    End Sub

    Private Sub LabelReset_MouseHover(sender As Object, e As EventArgs) Handles lblReset.MouseHover
        AllToolTip.SetToolTip(sender, "Click a radio button to clear the address and reset corresponding controls.")
    End Sub

    Private Sub LabelPollInterval_MouseHover(sender As Object, e As EventArgs) Handles lblPollInterval.MouseHover
        AllToolTip.SetToolTip(sender, "Poll Interval in milliseconds.")
    End Sub

    Private Sub LabelAutoRead_MouseHover(sender As Object, e As EventArgs) Handles lblAutoRead.MouseHover
        AllToolTip.SetToolTip(sender, "Perform automatic reads.")
    End Sub

    Private Sub PictureBox1_MouseHover(sender As Object, e As EventArgs) Handles PictureBox1.MouseHover
        AllToolTip.SetToolTip(sender, "https://github.com/kyle-github/libplctag" & Environment.NewLine & "https://github.com/mesta1/libplctag-csharp" & Environment.NewLine & "Attribution to any and all trademark owners as well.")
    End Sub

#End Region

End Class
