package main

import (
	"fmt"
	"net"
	"regexp"
	"strings"
)

type client struct {
	conn net.Conn
	name string
}

type room struct {
	name    string
	clients []client
}

var clients []client
var rooms []room
var maxPlayer = 2

func main() {
	buffer := make([]byte, 2048)

	ln, err := net.Listen("tcp", ":8080")
	if err != nil {
		fmt.Println(err)
		return
	}

	defer ln.Close()

	fmt.Println("TCP server is listening on port 8080...")

	for {
		conn, err := ln.Accept()
		if err != nil {
			fmt.Println(err)
			continue
		}

		go func(conn net.Conn) {
			addr := conn.RemoteAddr()

			for {
				n, err := conn.Read(buffer)
				if err != nil {
					fmt.Printf("%s has disconnected\n", addr)
					onDisconnect(conn)
					break
				}

				message := string(buffer[:n])

				if strings.HasPrefix(message, "/name") {
					//Get the name
					name := strings.TrimPrefix(message, "/name ")

					onNameRegister(name, conn)

					continue
				}

				if strings.HasPrefix(message, "/room") {
					//Get the room
					room := strings.TrimPrefix(message, "/room ")

					canDoRoom := onRoomRegister(room, conn)

					if canDoRoom {
						fmt.Println("Can do room", room)
						conn.Write([]byte(fmt.Sprintf("/room %s\n", room)))
					}

					continue
				}

				if strings.HasPrefix(message, "/start") {
					onStartGame(conn)
					continue
				}

				//Reaching this code means there are only game event messages left.
				//Handle game event messages by passing them to the other client without changing the message
				onGameEventMsg(message, conn)
			}
		}(conn)
	}
}

// Game Event
func onGameEventMsg(message string, conn net.Conn) {
	// Find the recipient
	isInRoom, roomIndex, clientInRoomIndex := doesRoomHaveClient(conn)

	if isInRoom {

		fmt.Println("Game Event in room ", roomIndex, ", with ", len(rooms[roomIndex].clients), "clients: ", message)

		for i := 0; i < maxPlayer; i++ {
			// Send the msg to all the recipient in the room, except if the receiving client is the sender
			if i != clientInRoomIndex && i < len(rooms[roomIndex].clients) {
				// Find the client and send the msg
				client := rooms[roomIndex].clients[i]

				client.conn.Write([]byte(fmt.Sprintf("%s\n", message)))
			}
		}
	}
}

// Registration
func onStartGame(conn net.Conn) bool {
	//Find the room and the index that the player currently is in
	isInRoom, _, clientInRoomIndex := doesRoomHaveClient(conn)

	if isInRoom {
		//Notify the client with their client index
		conn.Write([]byte(fmt.Sprintf("/playerCode %d\n", clientInRoomIndex)))

		return true
	}

	return false
}

func onRoomRegister(roomName string, conn net.Conn) bool {
	//Check if the room is already there or not
	isRoomExist, roomIndex := doesRoomExist(roomName)

	isConnExist, clientIndex := doesConsistConnection(conn)

	//Find the room that the player currently is in
	isInRoom, roomIndexOfClient, clientInRoomIndex := doesRoomHaveClient(conn)

	if isConnExist {
		switch {
		case isRoomExist && !isInRoom && clearString(roomName) != "EXIT":
			{
				//Add the client into the room
				rooms[roomIndex].clients = append(rooms[roomIndex].clients, clients[clientIndex])
			}
		case !isRoomExist && !isInRoom && clearString(roomName) != "EXIT":
			{
				//Create a new room, set the name, and put in the client there
				newRoom := room{}
				newRoom.name = roomName
				newRoom.clients = append(newRoom.clients, clients[clientIndex])

				//Add the new room into the room list
				rooms = append(rooms, newRoom)
			}
		case isInRoom:
			{
				if clearString(roomName) == "EXIT" {
					//Remove the client from the room
					rooms[roomIndexOfClient].clients = removeClient(rooms[roomIndexOfClient].clients, clientInRoomIndex)
				} else {
					//Cannot join any other room, because this client is already in another room
					return false
				}
			}
		}

		//Print out the rooms
		for _, room := range rooms {
			fmt.Printf("%s with %d players.\n", room.name, len(room.clients))
		}
		fmt.Println()

		return true
	}

	return false
}

func onNameRegister(name string, conn net.Conn) bool {
	//Check if the name is already in the client list or not
	isNameExist, _ := doesConsistClient(name)

	//Check if the conn is already in the client list or not
	isConnExist, connIndex := doesConsistConnection(conn)

	//Add the client into the list if both the name and the conn are not in the list
	if !isNameExist {
		//This is where the same client want to create a new name.
		if isConnExist {
			clients = removeClient(clients, connIndex)
		}

		clients = append(clients, client{conn: conn, name: name})

		//Notify the client - register successfully
		conn.Write([]byte(fmt.Sprintf("/name %s\n", name)))

		//Print out the clients
		for _, c := range clients {
			fmt.Printf("%s\n", c.name)
		}
		fmt.Println()

		return true
	} else {
		//Notify the client - register successfully
		conn.Write([]byte(fmt.Sprintf("/name Name already exists. Try another name\n")))

		return false
	}
}

// Disconnect
func onDisconnect(conn net.Conn) {
	//Find the room that the player currently is in
	isInRoom, roomIndexOfClient, clientInRoomIndex := doesRoomHaveClient(conn)

	if isInRoom {
		//Remove the client from the room
		rooms[roomIndexOfClient].clients = removeClient(rooms[roomIndexOfClient].clients, clientInRoomIndex)
	}

	//Remove the client from the client list
	doesConsist, clientIndex := doesConsistConnection(conn)

	if doesConsist {
		clients = removeClient(clients, clientIndex)

		//Print out the clients
		for _, c := range clients {
			fmt.Printf("%s\n", c.name)
		}
	}

	fmt.Println("Disconnect ", isInRoom, doesConsist)
}

// Ultility functions - for server

func doesRoomHaveClient(conn net.Conn) (bool, int, int) {
	for i := 0; i < len(rooms); i++ {
		for j := 0; j < len(rooms[i].clients); j++ {
			if conn == rooms[i].clients[j].conn {
				return true, i, j
			}
		}
	}
	return false, -1, -1
}

func doesRoomExist(roomName string) (bool, int) {
	for i := 0; i < len(rooms); i++ {
		if clearString(roomName) == clearString(rooms[i].name) {
			return true, i
		}
	}
	return false, -1
}

func doesConsistClient(name string) (bool, int) {
	for i := 0; i < len(clients); i++ {
		if clearString(name) == clearString(clients[i].name) {
			return true, i
		}
	}
	return false, -1
}

func doesConsistConnection(conn net.Conn) (bool, int) {
	for i := 0; i < len(clients); i++ {
		if conn == clients[i].conn {
			return true, i
		}
	}
	return false, -1
}

func removeClient(clientList []client, index int) []client {
	return append(clientList[:index], clientList[index+1:]...)
}

//Utils

var nonAlphanumericRegex = regexp.MustCompile(`[^a-zA-Z0-9 ]+`)

func clearString(str string) string {
	return nonAlphanumericRegex.ReplaceAllString(str, "")
}
