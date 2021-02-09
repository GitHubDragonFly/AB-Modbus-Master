<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.gbABMaster = New System.Windows.Forms.GroupBox()
        Me.cbPID = New System.Windows.Forms.ComboBox()
        Me.cbCustomString = New System.Windows.Forms.ComboBox()
        Me.lblCustomString = New System.Windows.Forms.Label()
        Me.tbPLCTag = New System.Windows.Forms.TextBox()
        Me.lblElementCount = New System.Windows.Forms.Label()
        Me.cbElementCount = New System.Windows.Forms.ComboBox()
        Me.lblDataType = New System.Windows.Forms.Label()
        Me.cbDataType = New System.Windows.Forms.ComboBox()
        Me.lblABPLCTag = New System.Windows.Forms.Label()
        Me.lblABPLCAddress = New System.Windows.Forms.Label()
        Me.tbABResultingAddress = New System.Windows.Forms.TextBox()
        Me.btnABOK = New System.Windows.Forms.Button()
        Me.gbModbus = New System.Windows.Forms.GroupBox()
        Me.lblPlctag = New System.Windows.Forms.Label()
        Me.lblEC = New System.Windows.Forms.Label()
        Me.cbEC = New System.Windows.Forms.ComboBox()
        Me.tbMFResultingAddress = New System.Windows.Forms.TextBox()
        Me.lblCharacter = New System.Windows.Forms.Label()
        Me.cbCharacter = New System.Windows.Forms.ComboBox()
        Me.lblBit = New System.Windows.Forms.Label()
        Me.cbBit = New System.Windows.Forms.ComboBox()
        Me.lblStringLength = New System.Windows.Forms.Label()
        Me.cbStringLength = New System.Windows.Forms.ComboBox()
        Me.lblModifier = New System.Windows.Forms.Label()
        Me.cbModifier = New System.Windows.Forms.ComboBox()
        Me.lblModbusRange = New System.Windows.Forms.Label()
        Me.lblAddress = New System.Windows.Forms.Label()
        Me.lblIO = New System.Windows.Forms.Label()
        Me.tbUserAddress = New System.Windows.Forms.TextBox()
        Me.lblModbus = New System.Windows.Forms.Label()
        Me.tbMResultingAddress = New System.Windows.Forms.TextBox()
        Me.btnModbusOK = New System.Windows.Forms.Button()
        Me.cbIO = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.gbABMaster.SuspendLayout()
        Me.gbModbus.SuspendLayout()
        Me.SuspendLayout()
        '
        'gbABMaster
        '
        Me.gbABMaster.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gbABMaster.Controls.Add(Me.Label1)
        Me.gbABMaster.Controls.Add(Me.cbPID)
        Me.gbABMaster.Controls.Add(Me.cbCustomString)
        Me.gbABMaster.Controls.Add(Me.lblCustomString)
        Me.gbABMaster.Controls.Add(Me.tbPLCTag)
        Me.gbABMaster.Controls.Add(Me.lblElementCount)
        Me.gbABMaster.Controls.Add(Me.cbElementCount)
        Me.gbABMaster.Controls.Add(Me.lblDataType)
        Me.gbABMaster.Controls.Add(Me.cbDataType)
        Me.gbABMaster.Controls.Add(Me.lblABPLCTag)
        Me.gbABMaster.Controls.Add(Me.lblABPLCAddress)
        Me.gbABMaster.Controls.Add(Me.tbABResultingAddress)
        Me.gbABMaster.Controls.Add(Me.btnABOK)
        Me.gbABMaster.ForeColor = System.Drawing.Color.DodgerBlue
        Me.gbABMaster.Location = New System.Drawing.Point(6, 6)
        Me.gbABMaster.Name = "gbABMaster"
        Me.gbABMaster.Size = New System.Drawing.Size(582, 205)
        Me.gbABMaster.TabIndex = 0
        Me.gbABMaster.TabStop = False
        Me.gbABMaster.Text = "AB Master"
        '
        'cbPID
        '
        Me.cbPID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPID.DropDownWidth = 64
        Me.cbPID.Enabled = False
        Me.cbPID.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbPID.FormattingEnabled = True
        Me.cbPID.Items.AddRange(New Object() {"None", "TM", "AM", "CM", "OL", "RG", "SC", "TF", "DA", "DB", "UL", "LL", "SP", "PV", "DN", "EN", "SPS", "KC", "Ti", "TD", "MAXS", "MINS", "ZCD", "CVH", "CVL", "LUT", "SPV", "CVP"})
        Me.cbPID.Location = New System.Drawing.Point(295, 140)
        Me.cbPID.MaxDropDownItems = 6
        Me.cbPID.Name = "cbPID"
        Me.cbPID.Size = New System.Drawing.Size(84, 28)
        Me.cbPID.TabIndex = 44
        '
        'cbCustomString
        '
        Me.cbCustomString.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCustomString.DropDownWidth = 64
        Me.cbCustomString.Enabled = False
        Me.cbCustomString.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCustomString.FormattingEnabled = True
        Me.cbCustomString.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130", "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145", "146", "147", "148", "149", "150", "151", "152", "153", "154", "155", "156", "157", "158", "159", "160", "161", "162", "163", "164", "165", "166", "167", "168", "169", "170", "171", "172", "173", "174", "175", "176", "177", "178", "179", "180", "181", "182", "183", "184", "185", "186", "187", "188", "189", "190", "191", "192", "193", "194", "195", "196", "197", "198", "199", "200"})
        Me.cbCustomString.Location = New System.Drawing.Point(225, 140)
        Me.cbCustomString.MaxDropDownItems = 6
        Me.cbCustomString.Name = "cbCustomString"
        Me.cbCustomString.Size = New System.Drawing.Size(64, 28)
        Me.cbCustomString.TabIndex = 43
        '
        'lblCustomString
        '
        Me.lblCustomString.AutoSize = True
        Me.lblCustomString.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCustomString.ForeColor = System.Drawing.Color.White
        Me.lblCustomString.Location = New System.Drawing.Point(86, 146)
        Me.lblCustomString.Name = "lblCustomString"
        Me.lblCustomString.Size = New System.Drawing.Size(133, 16)
        Me.lblCustomString.TabIndex = 42
        Me.lblCustomString.Text = "Custom String Length"
        Me.lblCustomString.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'tbPLCTag
        '
        Me.tbPLCTag.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbPLCTag.ForeColor = System.Drawing.Color.Black
        Me.tbPLCTag.Location = New System.Drawing.Point(26, 32)
        Me.tbPLCTag.Name = "tbPLCTag"
        Me.tbPLCTag.Size = New System.Drawing.Size(263, 31)
        Me.tbPLCTag.TabIndex = 26
        Me.tbPLCTag.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblElementCount
        '
        Me.lblElementCount.AutoSize = True
        Me.lblElementCount.ForeColor = System.Drawing.Color.White
        Me.lblElementCount.Location = New System.Drawing.Point(483, 14)
        Me.lblElementCount.Name = "lblElementCount"
        Me.lblElementCount.Size = New System.Drawing.Size(76, 13)
        Me.lblElementCount.TabIndex = 34
        Me.lblElementCount.Text = "Element Count"
        '
        'cbElementCount
        '
        Me.cbElementCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbElementCount.DropDownWidth = 73
        Me.cbElementCount.Enabled = False
        Me.cbElementCount.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbElementCount.FormattingEnabled = True
        Me.cbElementCount.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"})
        Me.cbElementCount.Location = New System.Drawing.Point(484, 31)
        Me.cbElementCount.MaxDropDownItems = 6
        Me.cbElementCount.Name = "cbElementCount"
        Me.cbElementCount.Size = New System.Drawing.Size(73, 33)
        Me.cbElementCount.TabIndex = 33
        '
        'lblDataType
        '
        Me.lblDataType.AutoSize = True
        Me.lblDataType.ForeColor = System.Drawing.Color.White
        Me.lblDataType.Location = New System.Drawing.Point(362, 14)
        Me.lblDataType.Name = "lblDataType"
        Me.lblDataType.Size = New System.Drawing.Size(54, 13)
        Me.lblDataType.TabIndex = 32
        Me.lblDataType.Text = "DataType"
        '
        'cbDataType
        '
        Me.cbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDataType.DropDownWidth = 183
        Me.cbDataType.Enabled = False
        Me.cbDataType.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbDataType.FormattingEnabled = True
        Me.cbDataType.Items.AddRange(New Object() {"Int8", "SINT", "UInt8", "USINT", "Int16", "INT", "UInt16", "UINT", "Int32", "DINT", "UInt32", "UDINT", "Float32", "REAL", "Int64", "LINT", "UInt64", "ULINT", "Float64", "LREAL", "Int128", "QDINT", "UInt128", "QUDINT", "BOOL", "BOOL Array", "String", "Custom String", "Timer", "Counter", "Control", "PID"})
        Me.cbDataType.Location = New System.Drawing.Point(295, 31)
        Me.cbDataType.MaxDropDownItems = 4
        Me.cbDataType.Name = "cbDataType"
        Me.cbDataType.Size = New System.Drawing.Size(183, 33)
        Me.cbDataType.TabIndex = 31
        '
        'lblABPLCTag
        '
        Me.lblABPLCTag.AutoSize = True
        Me.lblABPLCTag.ForeColor = System.Drawing.Color.White
        Me.lblABPLCTag.Location = New System.Drawing.Point(131, 14)
        Me.lblABPLCTag.Name = "lblABPLCTag"
        Me.lblABPLCTag.Size = New System.Drawing.Size(49, 13)
        Me.lblABPLCTag.TabIndex = 30
        Me.lblABPLCTag.Text = "PLC Tag"
        '
        'lblABPLCAddress
        '
        Me.lblABPLCAddress.AutoSize = True
        Me.lblABPLCAddress.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblABPLCAddress.ForeColor = System.Drawing.Color.White
        Me.lblABPLCAddress.Location = New System.Drawing.Point(23, 85)
        Me.lblABPLCAddress.Name = "lblABPLCAddress"
        Me.lblABPLCAddress.Size = New System.Drawing.Size(59, 32)
        Me.lblABPLCAddress.TabIndex = 29
        Me.lblABPLCAddress.Text = "PLC" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Address"
        Me.lblABPLCAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'tbABResultingAddress
        '
        Me.tbABResultingAddress.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbABResultingAddress.ForeColor = System.Drawing.Color.Black
        Me.tbABResultingAddress.Location = New System.Drawing.Point(88, 82)
        Me.tbABResultingAddress.Name = "tbABResultingAddress"
        Me.tbABResultingAddress.ReadOnly = True
        Me.tbABResultingAddress.Size = New System.Drawing.Size(469, 35)
        Me.tbABResultingAddress.TabIndex = 28
        Me.tbABResultingAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnABOK
        '
        Me.btnABOK.BackColor = System.Drawing.Color.Gainsboro
        Me.btnABOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnABOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnABOK.ForeColor = System.Drawing.Color.Blue
        Me.btnABOK.Location = New System.Drawing.Point(493, 134)
        Me.btnABOK.Name = "btnABOK"
        Me.btnABOK.Size = New System.Drawing.Size(64, 36)
        Me.btnABOK.TabIndex = 27
        Me.btnABOK.Text = "OK"
        Me.btnABOK.UseVisualStyleBackColor = False
        '
        'gbModbus
        '
        Me.gbModbus.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gbModbus.Controls.Add(Me.lblPlctag)
        Me.gbModbus.Controls.Add(Me.lblEC)
        Me.gbModbus.Controls.Add(Me.cbEC)
        Me.gbModbus.Controls.Add(Me.tbMFResultingAddress)
        Me.gbModbus.Controls.Add(Me.lblCharacter)
        Me.gbModbus.Controls.Add(Me.cbCharacter)
        Me.gbModbus.Controls.Add(Me.lblBit)
        Me.gbModbus.Controls.Add(Me.cbBit)
        Me.gbModbus.Controls.Add(Me.lblStringLength)
        Me.gbModbus.Controls.Add(Me.cbStringLength)
        Me.gbModbus.Controls.Add(Me.lblModifier)
        Me.gbModbus.Controls.Add(Me.cbModifier)
        Me.gbModbus.Controls.Add(Me.lblModbusRange)
        Me.gbModbus.Controls.Add(Me.lblAddress)
        Me.gbModbus.Controls.Add(Me.lblIO)
        Me.gbModbus.Controls.Add(Me.tbUserAddress)
        Me.gbModbus.Controls.Add(Me.lblModbus)
        Me.gbModbus.Controls.Add(Me.tbMResultingAddress)
        Me.gbModbus.Controls.Add(Me.btnModbusOK)
        Me.gbModbus.Controls.Add(Me.cbIO)
        Me.gbModbus.Enabled = False
        Me.gbModbus.ForeColor = System.Drawing.Color.DodgerBlue
        Me.gbModbus.Location = New System.Drawing.Point(2, 2)
        Me.gbModbus.Name = "gbModbus"
        Me.gbModbus.Size = New System.Drawing.Size(582, 205)
        Me.gbModbus.TabIndex = 42
        Me.gbModbus.TabStop = False
        Me.gbModbus.Text = "Modbus"
        '
        'lblPlctag
        '
        Me.lblPlctag.AutoSize = True
        Me.lblPlctag.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlctag.ForeColor = System.Drawing.Color.White
        Me.lblPlctag.Location = New System.Drawing.Point(67, 138)
        Me.lblPlctag.Name = "lblPlctag"
        Me.lblPlctag.Size = New System.Drawing.Size(53, 20)
        Me.lblPlctag.TabIndex = 40
        Me.lblPlctag.Text = "Plctag"
        '
        'lblEC
        '
        Me.lblEC.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblEC.Location = New System.Drawing.Point(480, 27)
        Me.lblEC.Name = "lblEC"
        Me.lblEC.Size = New System.Drawing.Size(76, 13)
        Me.lblEC.TabIndex = 39
        Me.lblEC.Text = "Element Count"
        Me.lblEC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cbEC
        '
        Me.cbEC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbEC.DropDownWidth = 73
        Me.cbEC.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbEC.FormattingEnabled = True
        Me.cbEC.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"})
        Me.cbEC.Location = New System.Drawing.Point(481, 44)
        Me.cbEC.MaxDropDownItems = 6
        Me.cbEC.Name = "cbEC"
        Me.cbEC.Size = New System.Drawing.Size(73, 33)
        Me.cbEC.TabIndex = 38
        '
        'tbMFResultingAddress
        '
        Me.tbMFResultingAddress.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMFResultingAddress.ForeColor = System.Drawing.Color.Black
        Me.tbMFResultingAddress.Location = New System.Drawing.Point(133, 131)
        Me.tbMFResultingAddress.Name = "tbMFResultingAddress"
        Me.tbMFResultingAddress.ReadOnly = True
        Me.tbMFResultingAddress.Size = New System.Drawing.Size(323, 35)
        Me.tbMFResultingAddress.TabIndex = 37
        Me.tbMFResultingAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblCharacter
        '
        Me.lblCharacter.AutoSize = True
        Me.lblCharacter.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblCharacter.Location = New System.Drawing.Point(415, 27)
        Me.lblCharacter.Name = "lblCharacter"
        Me.lblCharacter.Size = New System.Drawing.Size(53, 13)
        Me.lblCharacter.TabIndex = 36
        Me.lblCharacter.Text = "Character"
        '
        'cbCharacter
        '
        Me.cbCharacter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCharacter.DropDownWidth = 65
        Me.cbCharacter.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbCharacter.FormattingEnabled = True
        Me.cbCharacter.Items.AddRange(New Object() {" ", ".1", ".2", ".3", ".4", ".5", ".6", ".7", ".8", ".9", ".10", ".11", ".12", ".13", ".14", ".15", ".16", ".17", ".18", ".19", ".20", ".21", ".22", ".23", ".24", ".25", ".26", ".27", ".28", ".29", ".30", ".31", ".32", ".33", ".34", ".35", ".36", ".37", ".38", ".39", ".40", ".41", ".42", ".43", ".44", ".45", ".46", ".47", ".48", ".49", ".50", ".51", ".52", ".53", ".54", ".55", ".56", ".57", ".58", ".59", ".60", ".61", ".62", ".63", ".64", ".65", ".66", ".67", ".68", ".69", ".70", ".71", ".72", ".73", ".74", ".75", ".76", ".77", ".78", ".79", ".80", ".81", ".82", ".83", ".84", ".85", ".86", ".87", ".88", ".89", ".90", ".91", ".92", ".93", ".94", ".95", ".96", ".97", ".98", ".99", ".100"})
        Me.cbCharacter.Location = New System.Drawing.Point(410, 44)
        Me.cbCharacter.MaxDropDownItems = 6
        Me.cbCharacter.Name = "cbCharacter"
        Me.cbCharacter.Size = New System.Drawing.Size(65, 33)
        Me.cbCharacter.TabIndex = 35
        '
        'lblBit
        '
        Me.lblBit.AutoSize = True
        Me.lblBit.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblBit.Location = New System.Drawing.Point(225, 27)
        Me.lblBit.Name = "lblBit"
        Me.lblBit.Size = New System.Drawing.Size(19, 13)
        Me.lblBit.TabIndex = 34
        Me.lblBit.Text = "Bit"
        '
        'cbBit
        '
        Me.cbBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbBit.DropDownWidth = 59
        Me.cbBit.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbBit.FormattingEnabled = True
        Me.cbBit.Items.AddRange(New Object() {" ", ".0", ".1", ".2", ".3", ".4", ".5", ".6", ".7", ".8", ".9", ".10", ".11", ".12", ".13", ".14", ".15", ".16", ".17", ".18", ".19", ".20", ".21", ".22", ".23", ".24", ".25", ".26", ".27", ".28", ".29", ".30", ".31", ".32", ".33", ".34", ".35", ".36", ".37", ".38", ".39", ".40", ".41", ".42", ".43", ".44", ".45", ".46", ".47", ".48", ".49", ".50", ".51", ".52", ".53", ".54", ".55", ".56", ".57", ".58", ".59", ".60", ".61", ".62", ".63", ".64", ".65", ".66", ".67", ".68", ".69", ".70", ".71", ".72", ".73", ".74", ".75", ".76", ".77", ".78", ".79", ".80", ".81", ".82", ".83", ".84", ".85", ".86", ".87", ".88", ".89", ".90", ".91", ".92", ".93", ".94", ".95", ".96", ".97", ".98", ".99", ".100", ".101", ".102", ".103", ".104", ".105", ".106", ".107", ".108", ".109", ".110", ".111", ".112", ".113", ".114", ".115", ".116", ".117", ".118", ".119", ".120", ".121", ".122", ".123", ".124", ".125", ".126", ".127"})
        Me.cbBit.Location = New System.Drawing.Point(208, 44)
        Me.cbBit.MaxDropDownItems = 4
        Me.cbBit.Name = "cbBit"
        Me.cbBit.Size = New System.Drawing.Size(59, 33)
        Me.cbBit.TabIndex = 33
        '
        'lblStringLength
        '
        Me.lblStringLength.AutoSize = True
        Me.lblStringLength.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblStringLength.Location = New System.Drawing.Point(335, 27)
        Me.lblStringLength.Name = "lblStringLength"
        Me.lblStringLength.Size = New System.Drawing.Size(70, 13)
        Me.lblStringLength.TabIndex = 32
        Me.lblStringLength.Text = "String Length"
        '
        'cbStringLength
        '
        Me.cbStringLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbStringLength.DropDownWidth = 65
        Me.cbStringLength.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbStringLength.FormattingEnabled = True
        Me.cbStringLength.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"})
        Me.cbStringLength.Location = New System.Drawing.Point(338, 44)
        Me.cbStringLength.MaxDropDownItems = 6
        Me.cbStringLength.Name = "cbStringLength"
        Me.cbStringLength.Size = New System.Drawing.Size(65, 33)
        Me.cbStringLength.TabIndex = 31
        '
        'lblModifier
        '
        Me.lblModifier.AutoSize = True
        Me.lblModifier.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblModifier.Location = New System.Drawing.Point(282, 27)
        Me.lblModifier.Name = "lblModifier"
        Me.lblModifier.Size = New System.Drawing.Size(44, 13)
        Me.lblModifier.TabIndex = 30
        Me.lblModifier.Text = "Modifier"
        '
        'cbModifier
        '
        Me.cbModifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbModifier.DropDownWidth = 59
        Me.cbModifier.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbModifier.FormattingEnabled = True
        Me.cbModifier.Items.AddRange(New Object() {" ", "U", "S", "F", "L", "UL", "FQ", "LQ", "UQ", "LO", "UO"})
        Me.cbModifier.Location = New System.Drawing.Point(273, 44)
        Me.cbModifier.MaxDropDownItems = 4
        Me.cbModifier.Name = "cbModifier"
        Me.cbModifier.Size = New System.Drawing.Size(59, 33)
        Me.cbModifier.TabIndex = 29
        '
        'lblModbusRange
        '
        Me.lblModbusRange.AutoSize = True
        Me.lblModbusRange.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblModbusRange.ForeColor = System.Drawing.Color.SteelBlue
        Me.lblModbusRange.Location = New System.Drawing.Point(95, 176)
        Me.lblModbusRange.Name = "lblModbusRange"
        Me.lblModbusRange.Size = New System.Drawing.Size(412, 20)
        Me.lblModbusRange.TabIndex = 28
        Me.lblModbusRange.Text = "Modbus valid range: x00000 to x65534 (no offset applied)"
        '
        'lblAddress
        '
        Me.lblAddress.AutoSize = True
        Me.lblAddress.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblAddress.Location = New System.Drawing.Point(117, 27)
        Me.lblAddress.Name = "lblAddress"
        Me.lblAddress.Size = New System.Drawing.Size(45, 13)
        Me.lblAddress.TabIndex = 27
        Me.lblAddress.Text = "Address"
        '
        'lblIO
        '
        Me.lblIO.AutoSize = True
        Me.lblIO.ForeColor = System.Drawing.SystemColors.ControlLight
        Me.lblIO.Location = New System.Drawing.Point(43, 27)
        Me.lblIO.Name = "lblIO"
        Me.lblIO.Size = New System.Drawing.Size(23, 13)
        Me.lblIO.TabIndex = 26
        Me.lblIO.Text = "I/O"
        '
        'tbUserAddress
        '
        Me.tbUserAddress.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbUserAddress.Location = New System.Drawing.Point(80, 43)
        Me.tbUserAddress.MaxLength = 5
        Me.tbUserAddress.Name = "tbUserAddress"
        Me.tbUserAddress.Size = New System.Drawing.Size(122, 35)
        Me.tbUserAddress.TabIndex = 25
        Me.tbUserAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblModbus
        '
        Me.lblModbus.AutoSize = True
        Me.lblModbus.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblModbus.ForeColor = System.Drawing.Color.White
        Me.lblModbus.Location = New System.Drawing.Point(54, 100)
        Me.lblModbus.Name = "lblModbus"
        Me.lblModbus.Size = New System.Drawing.Size(66, 20)
        Me.lblModbus.TabIndex = 24
        Me.lblModbus.Text = "Modbus"
        '
        'tbMResultingAddress
        '
        Me.tbMResultingAddress.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMResultingAddress.ForeColor = System.Drawing.Color.Black
        Me.tbMResultingAddress.Location = New System.Drawing.Point(133, 91)
        Me.tbMResultingAddress.Name = "tbMResultingAddress"
        Me.tbMResultingAddress.ReadOnly = True
        Me.tbMResultingAddress.Size = New System.Drawing.Size(323, 35)
        Me.tbMResultingAddress.TabIndex = 23
        Me.tbMResultingAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnModbusOK
        '
        Me.btnModbusOK.BackColor = System.Drawing.Color.Gainsboro
        Me.btnModbusOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnModbusOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnModbusOK.ForeColor = System.Drawing.Color.Blue
        Me.btnModbusOK.Location = New System.Drawing.Point(469, 108)
        Me.btnModbusOK.Name = "btnModbusOK"
        Me.btnModbusOK.Size = New System.Drawing.Size(64, 36)
        Me.btnModbusOK.TabIndex = 22
        Me.btnModbusOK.Text = "OK"
        Me.btnModbusOK.UseVisualStyleBackColor = False
        '
        'cbIO
        '
        Me.cbIO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbIO.DropDownWidth = 40
        Me.cbIO.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbIO.FormattingEnabled = True
        Me.cbIO.Items.AddRange(New Object() {"0", "1", "3", "4"})
        Me.cbIO.Location = New System.Drawing.Point(34, 44)
        Me.cbIO.MaxDropDownItems = 4
        Me.cbIO.Name = "cbIO"
        Me.cbIO.Size = New System.Drawing.Size(40, 33)
        Me.cbIO.Sorted = True
        Me.cbIO.TabIndex = 21
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(386, 146)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(91, 16)
        Me.Label1.TabIndex = 45
        Me.Label1.Text = "PID Bit / Word"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.MidnightBlue
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(586, 208)
        Me.Controls.Add(Me.gbABMaster)
        Me.Controls.Add(Me.gbModbus)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(602, 247)
        Me.Name = "Form2"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "~ Select Tag Address"
        Me.gbABMaster.ResumeLayout(False)
        Me.gbABMaster.PerformLayout()
        Me.gbModbus.ResumeLayout(False)
        Me.gbModbus.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents gbABMaster As GroupBox
    Friend WithEvents tbPLCTag As TextBox
    Friend WithEvents lblElementCount As Label
    Friend WithEvents cbElementCount As ComboBox
    Friend WithEvents lblDataType As Label
    Friend WithEvents cbDataType As ComboBox
    Friend WithEvents lblABPLCTag As Label
    Friend WithEvents lblABPLCAddress As Label
    Friend WithEvents tbABResultingAddress As TextBox
    Friend WithEvents btnABOK As Button
    Friend WithEvents gbModbus As GroupBox
    Friend WithEvents lblCharacter As Label
    Friend WithEvents cbCharacter As ComboBox
    Friend WithEvents lblBit As Label
    Friend WithEvents cbBit As ComboBox
    Friend WithEvents lblStringLength As Label
    Friend WithEvents cbStringLength As ComboBox
    Friend WithEvents lblModifier As Label
    Friend WithEvents cbModifier As ComboBox
    Friend WithEvents lblModbusRange As Label
    Friend WithEvents lblAddress As Label
    Friend WithEvents lblIO As Label
    Friend WithEvents tbUserAddress As TextBox
    Friend WithEvents lblModbus As Label
    Friend WithEvents tbMResultingAddress As TextBox
    Friend WithEvents btnModbusOK As Button
    Friend WithEvents cbIO As ComboBox
    Friend WithEvents tbMFResultingAddress As TextBox
    Friend WithEvents lblEC As Label
    Friend WithEvents cbEC As ComboBox
    Friend WithEvents lblPlctag As Label
    Friend WithEvents cbCustomString As ComboBox
    Friend WithEvents lblCustomString As Label
    Friend WithEvents cbPID As ComboBox
    Friend WithEvents Label1 As Label
End Class
