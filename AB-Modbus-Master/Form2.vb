Public Class Form2

    Private ReadOnly AllToolTip As New ToolTip
    Private NoText As Boolean 'If NoText = True then don't process events while indexes and values are being set
    Private PIDSuffix As String = ""
    Public Txt As String = ""
    Public ElementCount As String = ""
    Public modbus As Boolean

#Region "Private Methods"

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If modbus Then
            gbABMaster.Enabled = False
            gbABMaster.Visible = False
            gbABMaster.Location = New Point(6, 6)

            gbModbus.Enabled = True
            gbModbus.Visible = True
            gbModbus.Location = New Point(2, 2)

            Text = "~ Select Modbus Address"
        Else
            gbModbus.Enabled = False
            gbModbus.Visible = False
            gbModbus.Location = New Point(4, 4)

            gbABMaster.Enabled = True
            gbABMaster.Visible = True
            gbABMaster.Location = New Point(2, 2)

            Text = "~ Select Tag Address"
        End If

        If Not String.IsNullOrWhiteSpace(Txt) Then
            If modbus Then
                InitializeAll()

                For i = 0 To cbIO.Items.Count - 1
                    If cbIO.Items.Item(i) = Txt.Substring(0, 1) Then
                        cbIO.SelectedIndex = cbIO.Items.IndexOf(cbIO.Items.Item(i))
                        Exit For
                    End If
                Next

                tbUserAddress.Text = Txt.Substring(1, 5)

                If Txt.Length > 6 Then
                    Dim extension = Txt.Substring(6)
                    If extension.IndexOfAny(".") <> -1 Then
                        If extension.IndexOfAny("F") <> -1 AndAlso extension.IndexOfAny("Q") <> -1 Then
                            SetBitIndex(extension, "FQ")
                            cbModifier.SelectedIndex = 6
                        ElseIf extension.IndexOfAny("F") <> -1 Then
                            SetBitIndex(extension, "F")
                            cbModifier.SelectedIndex = 3
                        ElseIf extension.IndexOfAny("S") <> -1 Then
                            SetBitIndex(extension, "S")
                            cbModifier.SelectedIndex = 2
                        ElseIf extension.IndexOfAny("U") <> -1 AndAlso extension.IndexOfAny("L") <> -1 Then
                            SetBitIndex(extension, "UL")
                            cbModifier.SelectedIndex = 5
                        ElseIf extension.IndexOfAny("U") <> -1 AndAlso extension.IndexOfAny("Q") <> -1 Then
                            SetBitIndex(extension, "UQ")
                            cbModifier.SelectedIndex = 8
                        ElseIf extension.IndexOfAny("U") <> -1 AndAlso extension.IndexOfAny("O") <> -1 Then
                            SetBitIndex(extension, "UO")
                            cbModifier.SelectedIndex = 10
                        ElseIf extension.IndexOfAny("L") <> -1 AndAlso extension.IndexOfAny("Q") <> -1 Then
                            SetBitIndex(extension, "LQ")
                            cbModifier.SelectedIndex = 7
                        ElseIf extension.IndexOfAny("L") <> -1 AndAlso extension.IndexOfAny("O") <> -1 Then
                            SetBitIndex(extension, "LO")
                            cbModifier.SelectedIndex = 9
                        ElseIf extension.IndexOfAny("L") <> -1 Then
                            SetBitIndex(extension, "L")
                            cbModifier.SelectedIndex = 4
                        ElseIf extension.IndexOfAny("U") <> -1 Then
                            SetBitIndex(extension, "U")
                            cbModifier.SelectedIndex = 1
                        Else
                            SetBitIndex(extension, "")
                            For i = 0 To cbModifier.Items.Count - 1
                                If cbModifier.Items.Item(i) = Txt.Substring(6) Then
                                    cbModifier.SelectedIndex = cbModifier.Items.IndexOf(cbModifier.Items.Item(i))
                                    Exit For
                                End If
                            Next
                        End If
                    Else
                        If extension.IndexOfAny("S") <> -1 Then
                            cbModifier.SelectedIndex = 2
                            For i = 0 To cbStringLength.Items.Count - 1
                                If cbStringLength.Items.Item(i) = extension.Substring(1) Then
                                    cbStringLength.SelectedIndex = cbStringLength.Items.IndexOf(cbStringLength.Items.Item(i))
                                    Exit For
                                End If
                            Next
                        Else
                            For i = 0 To cbModifier.Items.Count - 1
                                If cbModifier.Items.Item(i) = Txt.Substring(6) Then
                                    cbModifier.SelectedIndex = cbModifier.Items.IndexOf(cbModifier.Items.Item(i))
                                    Exit For
                                End If
                            Next
                        End If
                    End If
                End If
            Else
                cbDataType.Enabled = True
                cbElementCount.Enabled = True

                Dim vals = Txt.Split(New Char() {";"c})

                For k = 0 To vals.Length - 1
                    vals(k) = vals(k).Trim
                Next

                If vals(1) = "PID" Then
                    If vals(0).IndexOfAny(".") <> -1 Then
                        PIDSuffix = vals(0).Substring(vals(0).IndexOf(".") + 1)
                        vals(0) = vals(0).Substring(0, vals(0).IndexOf("."))
                    End If
                End If

                tbPLCTag.Text = vals(0)

                For i = 0 To cbDataType.Items.Count - 1
                    If cbDataType.Items.Item(i) = vals(1) Then
                        cbDataType.SelectedIndex = cbDataType.Items.IndexOf(cbDataType.Items.Item(i))

                        If Form1.cbCPUType.SelectedIndex = 0 OrElse Form1.cbCPUType.SelectedIndex = 1 OrElse Form1.cbCPUType.SelectedIndex = 6 Then
                            If cbDataType.SelectedItem.ToString = "BOOL" OrElse ((cbDataType.SelectedItem.ToString = "String" OrElse cbDataType.SelectedItem.ToString = "Custom String") AndAlso Not (tbPLCTag.Text.IndexOfAny("[") <> -1 AndAlso tbPLCTag.Text.IndexOfAny("]") <> -1)) Then
                                cbElementCount.SelectedIndex = 0
                                cbElementCount.Enabled = False
                            End If
                        ElseIf Form1.cbCPUType.SelectedIndex = 3 Then
                            If cbDataType.SelectedItem.ToString = "PID" Then
                                If PIDSuffix = "" Then
                                    cbElementCount.SelectedIndex = 0
                                    cbElementCount.Enabled = False

                                    cbPID.SelectedIndex = 0
                                Else
                                    cbElementCount.SelectedIndex = 0
                                    cbElementCount.Enabled = False

                                    For j = 0 To cbPID.Items.Count - 1
                                        If cbPID.Items.Item(j) = PIDSuffix Then
                                            cbPID.SelectedIndex = cbPID.Items.IndexOf(cbPID.Items.Item(j))
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                        End If

                        Exit For
                    End If
                Next

                For i = 0 To cbElementCount.Items.Count - 1
                    If cbElementCount.Items.Item(i) = vals(2) Then
                        cbElementCount.SelectedIndex = cbElementCount.Items.IndexOf(cbElementCount.Items.Item(i))
                        Exit For
                    End If
                Next

                tbABResultingAddress.Text = Txt
            End If
        Else
            If modbus Then
                InitializeAll()
            Else
                NoText = True
                tbPLCTag.Text = ""
                cbDataType.Enabled = False
                cbDataType.SelectedIndex = 0
                cbElementCount.Enabled = False
                cbElementCount.SelectedIndex = 0
                cbCustomString.Enabled = False
                cbCustomString.SelectedIndex = 0
                cbPID.Enabled = False
                cbPID.SelectedIndex = 0
                tbABResultingAddress.Text = ""
                btnABOK.Enabled = False
                NoText = False
            End If
        End If

        If modbus Then
            If cbStringLength.Enabled Then
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text & cbModifier.SelectedItem.ToString.Trim & cbStringLength.SelectedItem & cbBit.SelectedItem.ToString.Trim & cbCharacter.SelectedItem.ToString.Trim
            Else
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text & cbBit.SelectedItem.ToString.Trim & cbModifier.SelectedItem.ToString.Trim
            End If

            CheckModbusValues()
        End If
    End Sub

    Private Sub GroupBoxABMaster_VisibleChanged(sender As Object, e As EventArgs) Handles gbABMaster.VisibleChanged
        If gbABMaster.Visible Then
            ActiveControl = tbPLCTag
        End If
    End Sub

#Region "AB Master Methods"

    Private Sub TextBoxPLCTag_TextChanged(sender As Object, e As EventArgs) Handles tbPLCTag.TextChanged
        If Not NoText Then
            If Not String.IsNullOrWhiteSpace(tbPLCTag.Text) Then
                If Not cbDataType.Enabled Then
                    cbDataType.Enabled = True
                    ComboBoxDataType_SelectedIndexChanged(Me, EventArgs.Empty)
                End If

                If cbDataType.SelectedItem.ToString = "BOOL" OrElse (cbDataType.SelectedItem.ToString = "String" OrElse cbDataType.SelectedItem.ToString = "Custom String") AndAlso Not (tbPLCTag.Text.IndexOfAny("[") <> -1 AndAlso tbPLCTag.Text.IndexOfAny("]") <> -1) Then
                    If Form1.cbCPUType.SelectedIndex = 0 OrElse Form1.cbCPUType.SelectedIndex = 1 OrElse Form1.cbCPUType.SelectedIndex = 6 Then
                        cbElementCount.SelectedIndex = 0
                        cbElementCount.Enabled = False
                    End If
                ElseIf cbDataType.SelectedItem.ToString = "PID" Then
                    If cbPID.SelectedIndex = 0 Then
                        tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                    Else
                        tbABResultingAddress.Text = tbPLCTag.Text & "." & cbPID.SelectedItem & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                    End If
                Else
                    If Not cbElementCount.Enabled Then
                        cbElementCount.Enabled = True
                        ComboBoxElementCount_SelectedIndexChanged(Me, EventArgs.Empty)
                    End If
                End If

                If cbCustomString.Enabled Then
                    tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbCustomString.SelectedItem & "; " & cbElementCount.SelectedItem
                Else
                    tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                End If
            Else
                If cbDataType.Enabled Then
                    cbDataType.Enabled = False
                    cbDataType.SelectedIndex = 0
                End If

                If cbElementCount.Enabled Then
                    cbElementCount.Enabled = False
                    cbElementCount.SelectedIndex = 0
                End If

                tbABResultingAddress.Text = ""
            End If

            CheckValues()
        End If
    End Sub

    Private Sub ComboBoxDataType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbDataType.SelectedIndexChanged
        If Not NoText Then
            If cbDataType.Enabled Then
                If cbDataType.SelectedItem.ToString = "BOOL" Then
                    If Form1.cbCPUType.SelectedIndex = 0 Then
                        If cbCustomString.Enabled Then
                            cbCustomString.SelectedIndex = 0
                            cbCustomString.Enabled = False
                        End If

                        If cbPID.Enabled Then
                            cbPID.SelectedIndex = 0
                            cbPID.Enabled = False
                        End If

                        If cbElementCount.Enabled Then
                            cbElementCount.SelectedIndex = 0
                            cbElementCount.Enabled = False
                        End If
                    Else
                        MessageBox.Show("This data type is for the ControlLogix family of PLCs!")
                        cbDataType.SelectedIndex = 0
                    End If
                ElseIf cbDataType.SelectedItem.ToString = "BOOL Array" Then
                    If Form1.cbCPUType.SelectedIndex = 0 Then
                        cbElementCount.Enabled = True

                        If cbCustomString.Enabled Then
                            cbCustomString.SelectedIndex = 0
                            cbCustomString.Enabled = False
                        End If

                        If cbPID.Enabled Then
                            cbPID.SelectedIndex = 0
                            cbPID.Enabled = False
                        End If
                    Else
                        MessageBox.Show("This data type is for the ControlLogix family of PLCs!")
                        cbDataType.SelectedIndex = 0
                    End If
                ElseIf cbDataType.SelectedItem.ToString = "String" Then
                    If Form1.cbCPUType.SelectedIndex = 0 OrElse Form1.cbCPUType.SelectedIndex = 1 OrElse Form1.cbCPUType.SelectedIndex = 6 Then
                        If Not (tbPLCTag.Text.IndexOfAny("[") <> -1 AndAlso tbPLCTag.Text.IndexOfAny("]") <> -1) Then
                            If cbElementCount.Enabled Then
                                cbElementCount.SelectedIndex = 0
                                cbElementCount.Enabled = False
                            End If
                        End If

                        If cbCustomString.Enabled Then
                            cbCustomString.SelectedIndex = 0
                            cbCustomString.Enabled = False
                        End If

                        If cbPID.Enabled Then
                            cbPID.SelectedIndex = 0
                            cbPID.Enabled = False
                        End If
                    End If
                ElseIf cbDataType.SelectedItem.ToString = "Custom String" Then
                    If Form1.cbCPUType.SelectedIndex = 0 Then
                        If Not (tbPLCTag.Text.IndexOfAny("[") <> -1 AndAlso tbPLCTag.Text.IndexOfAny("]") <> -1) Then
                            If cbElementCount.Enabled Then
                                cbElementCount.SelectedIndex = 0
                                cbElementCount.Enabled = False
                            End If
                        End If

                        cbCustomString.Enabled = True

                        If cbPID.Enabled Then
                            cbPID.SelectedIndex = 0
                            cbPID.Enabled = False
                        End If
                    Else
                        MessageBox.Show("This data type is for the ControlLogix family of PLCs!")
                        cbDataType.SelectedIndex = 0
                    End If
                ElseIf cbDataType.SelectedItem.ToString = "PID" Then
                    If Form1.cbCPUType.SelectedIndex = 3 Then
                        cbPID.Enabled = True
                        cbPID.SelectedIndex = 0

                        cbElementCount.Enabled = False
                        cbElementCount.SelectedIndex = 0

                        cbCustomString.Enabled = False
                        cbCustomString.SelectedIndex = 0
                    Else
                        MessageBox.Show("This data type is for the MicroLogix family of PLCs!")
                        cbDataType.SelectedIndex = 0
                    End If
                Else
                    If cbCustomString.Enabled Then
                        cbCustomString.SelectedIndex = 0
                        cbCustomString.Enabled = False
                    End If

                    If cbPID.Enabled Then
                        cbPID.SelectedIndex = 0
                        cbPID.Enabled = False
                    End If

                    If Not cbElementCount.Enabled Then cbElementCount.Enabled = True

                End If

                If cbCustomString.Enabled Then
                    tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem & "; " & cbCustomString.SelectedItem
                ElseIf cbPID.Enabled Then
                    If cbPID.SelectedIndex = 0 Then
                        tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                    Else
                        tbABResultingAddress.Text = tbPLCTag.Text & "." & cbPID.SelectedItem & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                    End If
                Else
                    tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
                End If
            End If

            CheckValues()
        End If
    End Sub

    Private Sub ComboBoxPID_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbPID.SelectedIndexChanged
        If Not NoText Then
            If cbCustomString.Enabled Then
                cbCustomString.SelectedIndex = 0
                cbCustomString.Enabled = False
            End If

            If cbElementCount.Enabled Then
                cbElementCount.SelectedIndex = 0
                cbElementCount.Enabled = False
            End If

            If cbPID.SelectedIndex = 0 Then
                tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
            Else
                tbABResultingAddress.Text = tbPLCTag.Text & "." & cbPID.SelectedItem & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem
            End If
        End If
    End Sub

    Private Sub ComboBoxCustomString_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCustomString.SelectedIndexChanged
        If Not NoText Then
            If cbCustomString.Enabled Then
                tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem & "; " & cbCustomString.SelectedItem
            End If
        End If
    End Sub

    Private Sub ComboBoxElementCount_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbElementCount.SelectedIndexChanged
        If Not NoText Then
            If cbElementCount.Enabled Then
                tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "; " & cbElementCount.SelectedItem

                CheckValues()
            ElseIf cbPID.Enabled AndAlso cbPID.SelectedIndex > 0 Then
                tbABResultingAddress.Text = tbPLCTag.Text & "; " & cbDataType.SelectedItem & "." & cbPID.SelectedItem & "; " & cbElementCount.SelectedItem
            End If
        End If
    End Sub

    Private Sub CheckValues()
        If Not String.IsNullOrWhiteSpace(tbPLCTag.Text) Then
            btnABOK.Enabled = True
        Else
            btnABOK.Enabled = False
        End If
    End Sub

    Private Sub ButtonABOK_Click(sender As Object, e As EventArgs) Handles btnABOK.Click
        tbPLCTag.Focus()
    End Sub

#End Region

#Region "Modbus Methods"

    Private Sub InitializeAll()
        NoText = True
        cbIO.SelectedIndex = 0
        tbUserAddress.Text = "00000"
        cbBit.SelectedIndex = 0
        cbBit.Enabled = False
        cbModifier.SelectedIndex = 0
        cbModifier.Enabled = False
        cbStringLength.SelectedIndex = 0
        cbStringLength.Enabled = False
        cbCharacter.SelectedIndex = 0
        cbCharacter.Enabled = False
        If Not String.IsNullOrWhiteSpace(ElementCount) Then
            cbEC.SelectedIndex = CInt(ElementCount) - 1
        Else
            cbEC.SelectedIndex = 0
        End If
        NoText = False
    End Sub

    Private Sub SetBitIndex(ByVal txt As String, ByVal modifier As String)
        If Not String.IsNullOrWhiteSpace(modifier) Then
            If modifier = "S" Then
                cbBit.SelectedIndex = 0
                For i = 0 To cbStringLength.Items.Count - 1
                    If cbStringLength.Items.Item(i) = txt.Substring(1, txt.IndexOf(".") - 1) Then
                        cbStringLength.SelectedIndex = cbStringLength.Items.IndexOf(cbStringLength.Items.Item(i))
                        Exit For
                    End If
                Next
                For i = 0 To cbCharacter.Items.Count - 1
                    If cbCharacter.Items.Item(i) = txt.Substring(txt.IndexOf(".")) Then
                        cbCharacter.SelectedIndex = cbCharacter.Items.IndexOf(cbCharacter.Items.Item(i))
                        Exit For
                    End If
                Next
            Else
                For i = 0 To cbBit.Items.Count - 1
                    If cbBit.Items.Item(i) = txt.Substring(0, txt.IndexOf(modifier)) Then
                        cbBit.SelectedIndex = cbBit.Items.IndexOf(cbBit.Items.Item(i))
                        Exit For
                    End If
                Next
            End If
        Else
            For i = 0 To cbBit.Items.Count - 1
                If cbBit.Items.Item(i) = txt Then
                    cbBit.SelectedIndex = cbBit.Items.IndexOf(cbBit.Items.Item(i))
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub ComboBoxIO_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbIO.SelectedIndexChanged
        If Not NoText Then
            If cbIO.SelectedIndex = 0 OrElse cbIO.SelectedIndex = 1 Then
                cbBit.SelectedIndex = 0
                cbBit.Enabled = False
                cbModifier.SelectedIndex = 0
                cbModifier.Enabled = False
                cbStringLength.SelectedIndex = 0
                cbStringLength.Enabled = False
            Else
                cbModifier.SelectedIndex = 0
                cbBit.SelectedIndex = 0
                cbBit.Enabled = True
                cbModifier.Enabled = True
            End If

            tbMResultingAddress.Text = cbIO.Text & tbUserAddress.Text.PadLeft(5, "0"c)

            CheckModbusValues()
        End If
    End Sub

    Private Sub ComboBoxBit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbBit.SelectedIndexChanged
        If Not NoText Then
            If cbModifier.SelectedIndex = 2 Then 'String
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text & cbModifier.SelectedItem.ToString.Trim & cbStringLength.SelectedItem & cbCharacter.SelectedItem.ToString.Trim
            Else
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text & cbBit.SelectedItem.ToString.Trim & cbModifier.SelectedItem.ToString.Trim

                If cbBit.SelectedIndex > 0 Then
                    Dim bitNumber = CInt(cbBit.SelectedItem.ToString.Substring(1))
                    If (((bitNumber + cbEC.SelectedIndex) < 16) AndAlso (cbModifier.SelectedIndex <> 2)) OrElse
                        (((bitNumber + cbEC.SelectedIndex) > 15) AndAlso ((bitNumber + cbEC.SelectedIndex) < 32) AndAlso cbModifier.SelectedIndex > 2) OrElse
                        (((bitNumber + cbEC.SelectedIndex) > 31) AndAlso (cbModifier.SelectedIndex > 5)) OrElse
                        (((bitNumber + cbEC.SelectedIndex) > 63) AndAlso (cbModifier.SelectedIndex = 9 OrElse cbModifier.SelectedIndex = 10)) Then
                        tbMResultingAddress.BackColor = Color.LimeGreen
                    Else
                        tbMResultingAddress.BackColor = Color.Red
                    End If
                Else
                    CheckModbusValues()
                End If
            End If
        End If
    End Sub

    Private Sub ComboBoxModifier_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbModifier.SelectedIndexChanged
        If Not NoText Then
            If cbModifier.SelectedIndex = 2 Then 'String
                cbBit.Enabled = False
                cbStringLength.Enabled = True
                cbCharacter.Enabled = True
            Else
                cbBit.Enabled = True
                cbStringLength.Enabled = False
                cbCharacter.Enabled = False
            End If

            ComboBoxEC_SelectedIndexChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub ComboBoxString_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbStringLength.SelectedIndexChanged, cbCharacter.SelectedIndexChanged
        If Not NoText Then
            If cbStringLength.Enabled Then
                If cbCharacter.SelectedIndex > 0 Then
                    If Not cbEC.Enabled Then
                        cbEC.SelectedIndex = 0
                        cbEC.Enabled = True
                    End If

                    If (cbCharacter.SelectedIndex + cbEC.SelectedIndex + 1) < (cbStringLength.SelectedIndex + 3) Then
                        tbMResultingAddress.BackColor = Color.LimeGreen
                    Else
                        tbMResultingAddress.BackColor = Color.Red
                    End If
                Else
                    cbEC.SelectedIndex = 0
                    cbEC.Enabled = False

                    If (CInt(tbUserAddress.Text) + 2 * (cbStringLength.SelectedIndex + 1) + cbEC.SelectedIndex - 1) < 65536 Then
                        tbMResultingAddress.BackColor = Color.LimeGreen
                    Else
                        tbMResultingAddress.BackColor = Color.Red
                    End If
                End If

                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text.PadLeft(5, "0"c) & cbModifier.SelectedItem.ToString.Trim & cbStringLength.SelectedItem & cbCharacter.SelectedItem.ToString.Trim
            End If
        End If
    End Sub

    Private Sub ComboBoxEC_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbEC.SelectedIndexChanged
        If Not NoText Then
            If cbStringLength.Enabled Then
                ComboBoxString_SelectedIndexChanged(Me, EventArgs.Empty)

                ' Split string based on semicolon delimiter (split parts = PLC Address, Data Type, Element Count, StringLength)
                Dim stringValues = tbMFResultingAddress.Text.Split(New Char() {";"c})

                tbMFResultingAddress.Text = stringValues(0) & ";" & stringValues(1) & "; " & cbEC.SelectedItem.ToString & ";" & stringValues(3)
            Else
                ComboBoxBit_SelectedIndexChanged(Me, EventArgs.Empty)
                tbMFResultingAddress.Text = tbMFResultingAddress.Text.Substring(0, tbMFResultingAddress.Text.LastIndexOf(";") + 2) & cbEC.SelectedItem.ToString
            End If
        End If
    End Sub

    Private Sub TextBoxUserAddress_TextChanged(sender As Object, e As EventArgs) Handles tbUserAddress.TextChanged
        If Not NoText Then
            For Each ch As Char In tbUserAddress.Text
                'Allow only digits 0 to 9
                If Asc(ch) < 48 OrElse Asc(ch) > 57 Then
                    MessageBox.Show("Only numbers are allowed!")
                    tbUserAddress.Text = "00000"
                    SetValues()
                    Exit Sub
                End If
            Next

            If cbStringLength.Enabled Then
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text.PadLeft(5, "0"c) & cbModifier.SelectedItem.ToString & cbStringLength.SelectedItem & cbCharacter.SelectedItem.ToString.Trim
            Else
                tbMResultingAddress.Text = cbIO.SelectedItem & tbUserAddress.Text.PadLeft(5, "0"c) & cbBit.SelectedItem.ToString.Trim & cbModifier.SelectedItem.ToString.Trim
            End If

            CheckModbusValues()
        End If
    End Sub

    Private Sub TextBoxMResultingAddress_BackColorChanged(sender As Object, e As EventArgs) Handles tbMResultingAddress.BackColorChanged
        tbMFResultingAddress.BackColor = tbMResultingAddress.BackColor

        If tbMResultingAddress.BackColor = Color.Red Then
            btnModbusOK.Enabled = False
        Else
            btnModbusOK.Enabled = True
        End If
    End Sub

    Private Sub TextBoxMResultingAddress_TextChanged(sender As Object, e As EventArgs) Handles tbMResultingAddress.TextChanged
        SetValues()
    End Sub

    Private Sub ButtonModbusOK_Click(sender As Object, e As EventArgs) Handles btnModbusOK.Click
        ElementCount = ""
    End Sub

    Private Sub CheckModbusValues()
        Dim address As Integer
        If Integer.TryParse(tbUserAddress.Text, address) Then
            If address < 0 OrElse (cbModifier.SelectedIndex < 2 AndAlso address > 65534) OrElse
                (cbModifier.SelectedIndex > 2 AndAlso cbModifier.SelectedIndex < 6 AndAlso address > 65533) OrElse
                ((cbModifier.SelectedIndex = 6 OrElse cbModifier.SelectedIndex = 7 OrElse cbModifier.SelectedIndex = 8) AndAlso address > 65531) OrElse
                ((cbModifier.SelectedIndex = 9 OrElse cbModifier.SelectedIndex = 10) AndAlso address > 65527) OrElse
                (cbStringLength.Enabled AndAlso (address + Math.Ceiling((cbStringLength.SelectedIndex + 1) / 2)) > 65535) Then
                tbMResultingAddress.BackColor = Color.Red
            Else
                tbMResultingAddress.BackColor = Color.LimeGreen
            End If
        Else
            tbMResultingAddress.BackColor = Color.Red
        End If
    End Sub

    Private Sub SetValues()
        Select Case cbIO.SelectedIndex
            Case 0
                tbMFResultingAddress.Text = "co"
            Case 1
                tbMFResultingAddress.Text = "di"
            Case 2
                tbMFResultingAddress.Text = "ir"
            Case Else
                tbMFResultingAddress.Text = "hr"
        End Select

        If cbIO.SelectedIndex < 2 Then '0 or 1 - boolean addresses
            tbMFResultingAddress.Text &= CInt(tbUserAddress.Text) & "; BOOL; " & cbEC.SelectedItem.ToString
        Else '3 or 4 - register addresses
            If cbModifier.SelectedIndex = 2 Then 'String
                If cbCharacter.SelectedIndex = 0 Then
                    lblEC.Text = "Element Count"
                    cbEC.SelectedIndex = 0
                    cbEC.Enabled = False
                    tbMFResultingAddress.Text &= CInt(tbUserAddress.Text) & DataType(cbModifier.SelectedIndex) & "; " & cbEC.SelectedItem.ToString & "; " & cbStringLength.SelectedItem.ToString
                Else
                    lblEC.Text = "Char Count"
                    cbEC.Enabled = True
                    tbMFResultingAddress.Text &= CInt(tbUserAddress.Text) & "/" & cbCharacter.SelectedIndex & DataType(cbModifier.SelectedIndex) & "; " & cbEC.SelectedItem.ToString & "; " & cbStringLength.SelectedItem.ToString
                End If
            Else
                If cbBit.SelectedIndex = 0 Then
                    lblEC.Text = "Element Count"
                    tbMFResultingAddress.Text &= CInt(tbUserAddress.Text)
                Else
                    lblEC.Text = "Bit Count"
                    tbMFResultingAddress.Text &= CInt(tbUserAddress.Text) & "/" & cbBit.SelectedIndex - 1
                End If

                tbMFResultingAddress.Text &= DataType(cbModifier.SelectedIndex) & "; " & cbEC.SelectedItem.ToString
            End If
        End If
    End Sub

    Private Function DataType(ModifierSelectedIndex As Integer) As String
        Select Case ModifierSelectedIndex
            Case 0
                Return "; Int16"
            Case 1
                Return "; UInt16"
            Case 2
                Return "; String"
            Case 3
                Return "; Float32"
            Case 4
                Return "; Int32"
            Case 5
                Return "; UInt32"
            Case 6
                Return "; Float64"
            Case 7
                Return "; Int64"
            Case 8
                Return "; UInt64"
            Case 9
                Return "; Int128"
            Case 10
                Return "; UInt128"
            Case Else
                Return Nothing
        End Select
    End Function

#End Region

#End Region

#Region "ToolTips"

    Private Sub LabelElementCount_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblElementCount.MouseHover, lblEC.MouseHover
        AllToolTip.SetToolTip(sender, "If bit/character operation then it represents the number of consecutive bits/characters to process.")
    End Sub

    Private Sub LabelIO_MouseHover(sender As Object, e As EventArgs) Handles lblIO.MouseHover
        AllToolTip.SetToolTip(lblIO, "0 - Coils" & Environment.NewLine & "1 - Discrete Inputs" & Environment.NewLine & "3 - Input Registers" & Environment.NewLine & "4 - Holding Registers")
    End Sub

    Private Sub LabelModifier_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblModifier.MouseHover
        AllToolTip.SetToolTip(lblModifier, "F = Float32" & Environment.NewLine & "L = Int32" & Environment.NewLine & "S = String" & Environment.NewLine & "U = UInt16" & Environment.NewLine & "UL = UInt32" & Environment.NewLine & "FQ = Float64" & Environment.NewLine & "LQ = Int64" & Environment.NewLine & "UQ = UInt64" & Environment.NewLine & "LO = Int128" & Environment.NewLine & "UO = UInt128")
    End Sub

    Private Sub LabelCharacter_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblCharacter.MouseHover
        AllToolTip.SetToolTip(lblCharacter, "A character from the string")
    End Sub

    Private Sub LabelBit_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblBit.MouseHover
        AllToolTip.SetToolTip(lblBit, "A bit from the 16 / 32 / 64 / 128 bit number")
    End Sub

#End Region

End Class