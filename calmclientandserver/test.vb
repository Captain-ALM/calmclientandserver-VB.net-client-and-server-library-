Imports System.Net

Friend Module test
    Sub main()
        Dim server As New server(IPAddress.Parse("192.168.1.144"), 100)
        Dim client As New client()
        writeln("Start Server: " & server.Start("", EncryptionMethod.none))
        AddHandler server.ClientDisconnect, AddressOf event_handle_cd
        AddHandler server.ClientMessage, AddressOf event_handle_cm
        AddHandler server.ClientConnect, AddressOf event_handle_cc
        Threading.Thread.Sleep(1000)
        writeln("Check Server: " & client.CheckServer("192.168.1.144", 100))
        Threading.Thread.Sleep(1000)
        If client.CheckServer("192.168.1.144", 100) Then
            AddHandler client.ServerDisconnect, AddressOf event_handle_sd
            AddHandler client.ServerMessage, AddressOf event_handle_sm
            Threading.Thread.Sleep(1000)
            writeln("Connect Client: " & client.Connect("hello", "192.168.1.144", 100))
            Threading.Thread.Sleep(2000)
            writeln("Client Send Msg: " & client.Send(New packet(1, client.Name, New List(Of String), "test", "hello _ world")))
            Threading.Thread.Sleep(2000)
            'Console.ReadKey()
            writeln("Disconnect Client From Server: " & server.Disconnect(server.connected_clients(0)))
            Threading.Thread.Sleep(3000)
            server.Stop()
            writeln("Stop Server: ")
            server.flush()
            Threading.Thread.Sleep(1000)
            'writeln("Disconnect Client: " & client.Disconnect())
            'Threading.Thread.Sleep(1000)
            client.flush()
        End If
        'writeln("Stop Server: " & server.Stop())
        Threading.Thread.Sleep(1000)
        writeln("Check Server: " & client.CheckServer("192.168.1.144", 100))
        Threading.Thread.Sleep(1000)
        Console.ReadKey()
    End Sub

    Public Sub writeln(dat As String)
        Console.WriteLine("INFO: " & dat)
        Trace.WriteLine("INFO: " & dat)
    End Sub

    Public Sub event_handle_sd()
        writeln("Client -x Server Disconnect")
    End Sub

    Public Sub event_handle_cd(cl As String)
        writeln("Client " & cl & " x- Server Disconnect")
    End Sub

    Public Sub event_handle_cc(cl As String)
        writeln("Client " & cl & " -- Server Connect")
    End Sub

    Public Sub event_handle_sm(data As packet)
        writeln("Server Message: " & data.stringdata(""))
    End Sub

    Public Sub event_handle_cm(cl As String, data As packet)
        writeln("Client " & cl & " Message: " & data.stringdata(""))
    End Sub
End Module
