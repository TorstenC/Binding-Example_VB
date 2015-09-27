Imports System.Dynamic                  'ExpandoObject
Imports System.Collections.Generic      'EqualityComparer
Imports System.ComponentModel           'PropertyChanged
Imports System.Runtime.CompilerServices 'CallerMemberName
Imports System.Diagnostics              'TraceSource
Imports System.Timers                   'Timer, Elapsed, ElapsedEventArgs
Imports System.Xml                      'XmlElement
Imports System.Windows                  'RoutedEventArgs, Window.Loaded
Imports System.Windows.Controls         'HeaderedItemsControl
Imports System                          'EventArgs

Class MainWindow
    Public WithEvents ViewModel As MainWindowViewModel ' Vielleicht hier ein "New"? Siehe unten.
    Protected Shared DiagTrace As New TraceSource("MainWindow")
    ''' <summary>
    ''' New() wird nur zur Design-Time genutzt. Zur Run-Time wird MainWindow mit New(…) und .Show() erzeugt
    ''' Alternativ kann mit "DesignerProperties.GetIsInDesignMode(New DependencyObject())" unterschieden werden
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        If Me.ViewModel Is Nothing Then Me.ViewModel = New MainWindowViewModel
        MainWindow.DiagTrace.TraceEvent(TraceEventType.Verbose, 1, "New() ohne parameter")
    End Sub
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Me.DataContext Is Nothing Then Me.DataContext = Me.ViewModel
        'xxx eigentlich:
        ' DataContext = "{Binding Path=ViewModel, RelativeSource={RelativeSource Self}}",
        ' das geht aber nur mit "Public Property ViewModel As New MainWindowViewModel"
    End Sub
    Private Sub LöseEinEreignisAus_Click(sender As Object, e As RoutedEventArgs)
        Dim c As Control = e.Source
        'DiagTrace.TraceInformation("LöseEinEreignisAus_Click OriginalSource: " & e.OriginalSource.ToString)
        'DiagTrace.TraceInformation("LöseEinEreignisAus_Click         Source: " & e.Source.ToString)
        'DiagTrace.TraceInformation("LöseEinEreignisAus_Click         sender: " & sender.ToString)
        DiagTrace.TraceInformation("LöseEinEreignisAus_Click            UID: " & c.Uid)
    End Sub
    Private Sub ViewModel_XMLNodeChanged(sender As Object, e As XmlNodeChangedEventArgs) Handles ViewModel.XMLNodeChanged
        'TODO suche XPath: http://stackoverflow.com/a/241291/3303008
        'DiagTrace.TraceInformation("XMLNodeChanged " & e.Node.NodeType.ToString & " " & e.Node.LocalName & ": " & e.OldValue & " -> " & e.NewValue)
        Dim xE As XmlElement = DirectCast(e.Node.ParentNode, XmlAttribute).OwnerElement ' Text -> Attribut -> Element
        DiagTrace.TraceInformation("Property Changed: " & xE.GetAttribute("uid") & " -> " & e.NewValue)
    End Sub
    Private Sub ComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim Combo As ComboBox = sender
        Dim xE As XmlElement = Combo.SelectedItem
        If xE IsNot Nothing Then
            DiagTrace.TraceInformation("SelectionChanged: " & Combo.Uid & " -> " & xE.GetAttribute("uid"))
        End If
    End Sub

    Private Sub ContextMenu_MouseRightButtonDown(sender As Object, e As Input.MouseButtonEventArgs)
        Dim c As FrameworkElement = e.Source
        DiagTrace.TraceInformation("LöseEinEreignisAus_Click OriginalSource: " & e.OriginalSource.ToString)
        DiagTrace.TraceInformation("LöseEinEreignisAus_Click         Source: " & e.Source.ToString)
        DiagTrace.TraceInformation("LöseEinEreignisAus_Click         sender: " & sender.ToString)
        DiagTrace.TraceInformation("LöseEinEreignisAus_Click            UID: " & c.Uid)
    End Sub
End Class
#Region "code behind"
'siehe http://stackoverflow.com/a/13753446/3303008
Public Class MyHierarchicalViewItem
    Inherits HeaderedItemsControl
    Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
        Return TypeOf item Is MyHierarchicalViewItem
    End Function
    Protected Overrides Function GetContainerForItemOverride() As DependencyObject
        Return New MyHierarchicalViewItem()
    End Function
End Class
#End Region

''' <summary>
''' Klassen-Vorlage (besser in einer Extra-Datei)
''' </summary>
Public Class StrictViewModel
    Implements INotifyPropertyChanged
    ' Properties
    Protected CurrentTime_ As Date
    Public Property CurrentTime As Date
        Set(value As Date)
            SetField(CurrentTime_, value)
        End Set
        Get
            Return CurrentTime_
        End Get
    End Property

    ' boiler-plate
    Protected Shared DiagTrace As New TraceSource("StrictViewModel")
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Overridable Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    Protected Function SetField(Of T)(ByRef field As T, value As T, <CallerMemberName> Optional propertyName As String = Nothing) As Boolean
        If EqualityComparer(Of T).[Default].Equals(field, value) Then
            Return False
        End If
        StrictViewModel.DiagTrace.TraceEvent(TraceEventType.Verbose, 1, propertyName & ": " & field.ToString & " -> " & value.ToString)
        field = value
        OnPropertyChanged(propertyName)
        Return True
    End Function
End Class
''' <summary>
''' Klassen-Vorlage (besser in einer Extra-Datei)
''' </summary>
Public Class MainWindowViewModel
    'nicht nötig?:
    'Implements INotifyPropertyChanged
    'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected WithEvents ViewModelXML_ As XmlDocument
    Public Property ViewModelXML As XmlDocument
        Set(value As XmlDocument)
            Me.ViewModelXML_ = value
        End Set
        Get
            Return Me.ViewModelXML_
        End Get
    End Property
    Public Property ViewModelExpando As Object 'ExpandoObject
    Public Property ViewModelStrict As StrictViewModel
    Protected WithEvents DesignTimeTimer As New Timer With {.Enabled = True, .Interval = 4999}
    Private Sub DesignTimeTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles DesignTimeTimer.Elapsed
        Me.ViewModelExpando.CurrentTime = Date.Now
        Me.ViewModelStrict.CurrentTime = Date.Now
    End Sub
    Public Sub New()
        Me.ViewModelXML = New XmlDocument
        Me.ViewModelXML.LoadXml(
            <?xml version="1.0" encoding="UTF-8"?>
            <Folder name="root1" secondAttribute="zwei">
                <Expander name="1">

                </Expander>
                <Folder name="2">
                    <Property name="Auto Item" uid=":CURSor:AUTO:ITEM" index="0">
                        <Enum name="Off" uid="OFF"/>
                        <Enum name="Item 1" uid="ITEM1"/>
                        <Enum name="Item 2" uid="ITEM2"/>
                        <Enum name="Item 3" uid="ITEM3"/>
                        <Enum name="Item 4" uid="ITEM4"/>
                        <Enum name="Item 5" uid="ITEM5"/>
                    </Property>
                </Folder>
                <Folder name="3">Mixed Content<Folder name="1.1">Beispieltext</Folder>
                </Folder>
            </Folder>.ToString)
        Me.ViewModelStrict = New StrictViewModel
        Me.ViewModelExpando = New ExpandoObject
        Me.ViewModelExpando.CurrentTime = Date.Now
    End Sub
    Private Sub ViewModelXML_NodeChanging(sender As Object, e As XmlNodeChangedEventArgs) Handles ViewModelXML_.NodeChanging
        'to do extra checking and, if necessary, throw an exception to stop the operation
    End Sub
    Public Event XMLNodeChanged As XmlNodeChangedEventHandler
    Private Sub ViewModelXML_NodeChanged(sender As Object, e As XmlNodeChangedEventArgs) Handles ViewModelXML_.NodeChanged
        RaiseEvent XMLNodeChanged(sender, e)
    End Sub
End Class