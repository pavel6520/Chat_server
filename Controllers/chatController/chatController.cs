using ConnectionWorker;
using ConnectionWorker.Helpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class chatController : ControllerWorker {
	public void Init() {
		if (_helper.isSecureConnection) {
			if (!_helper.Auth.Status) {
				_helper.AnswerRedirect("/auth/login");
			}
		}
		else {
			_helper.AnswerRedirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void indexAction() {
		if (_helper.returnType == ReturnType.DefaultContent) {
			if (_helper.Request.HttpMethod == "POST") {
				_helper.Render.DissableRender();
				JObject json;
				try {
					if (_helper.Request.Content.Length > 0) {
						json = _helper.GetJsonContent();
						string type = json["type"].Value<string>();
						string act = json["act"].Value<string>();
						var connection = new MySqlConnection(_helper.dbConnectString);
						switch (type) {
							case "public": {
									switch (act) {
										case "get": {
												int count = json["count"].Value<int>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, id, idh, message, del FROM chat.publicmessage " +
													$"order by id*256+idh desc limit {(count > 50 ? 50 : count)}", connection);
												try {
													var reader = command.ExecuteReader();
													JArray array = new JArray();
													Stack<JObject> tmp = new Stack<JObject>();
													while (reader.Read()) {
														if (!Convert.ToBoolean(reader["del"])) {
															tmp.Push(new JObject {
															{ "login", JToken.FromObject(reader["login"]) },
															{ "id", JToken.FromObject(reader["id"]) },
															{ "idh", JToken.FromObject(reader["idh"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														});
														}
													}
													reader.Close();
													while (tmp.Count > 0) {
														array.Add(tmp.Pop());
													}
													EchoJson(new { state = true, list = array });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "getone": {
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, message, del FROM chat.publicmessage where del = 0 and id=@id and idh=@idh", connection);
												command.Parameters.AddWithValue("@id", json["id"].Value<string>());
												command.Parameters.AddWithValue("@idh", json["idh"].Value<string>());
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													if (reader.Read()) {
														JObject o = new JObject {
															{ "login", JToken.FromObject(reader["login"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														};
														EchoJson(new { state = true, o = o });
													}
													else {
														EchoJson(new { state = false, code = 0 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "add": {
												connection.Open();
												ulong dc = (ulong) new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()/1000;
												MySqlCommand command = new MySqlCommand("chat.publicmessage_add", connection) {
													CommandType = System.Data.CommandType.StoredProcedure };
												command.Parameters.AddWithValue("INl", _helper.Auth.Login);
												command.Parameters.AddWithValue("INid", dc);
												command.Parameters.AddWithValue("INm", json["message"].Value<string>());
												command.Parameters.Add("OUTidh", MySqlDbType.UByte);
												command.Parameters["OUTidh"].Direction = System.Data.ParameterDirection.Output;
												try {
													command.Prepare();
													command.ExecuteNonQuery();
													JObject o = new JObject {
														{ "type", type },
														{ "act", "add" },
														{ "id", dc },
														{ "idh", (byte)command.Parameters["OUTidh"].Value }
													};
													_helper.WShelper.ActsForAll = true;
													_helper.WShelper.Acts.Add(new WebSocketAct() { Body = JsonConvert.SerializeObject(o) });
													EchoJson(new { state = true });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "del": {
												break;
											}
									}
									break;
								}
							case "dialog": {
									switch (act) {
										case "create": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												string loginr = json["login"].Value<string>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("dialog_create", connection) {
													CommandType = System.Data.CommandType.StoredProcedure
												};
												command.Parameters.AddWithValue("@INlogins", _helper.Auth.Login);
												command.Parameters.AddWithValue("@INloginr", loginr);
												command.Parameters.AddWithValue("@INdc", dc);
												command.Prepare();
												try {
													object res = command.ExecuteScalar();
													if ((long)res == 1) {
														JObject o = new JObject {
															{ "type", type },
															{ "act", act },
															{ "login", _helper.Auth.Login },
															{ "dc", dc }
														};
														var WSact = new WebSocketAct() { Body = JsonConvert.SerializeObject(o) };
														WSact.Recepients.Add(loginr);
														_helper.WShelper.ActsForAll = false;
														_helper.WShelper.Acts.Add(WSact);
														EchoJson(new { state = true });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "invite": {
												connection.Open();
												MySqlCommand command = new MySqlCommand("select d1.logins, d1.dc from dialog d1 left join(select d3.logins, d3.loginr from dialog d3 where logins = @recipient) d2 on d1.logins = d2.loginr and d1.loginr = d2.logins where d1.loginr = @recipient and d2.logins is null", connection);
												command.Parameters.AddWithValue("@recipient", _helper.Auth.Login);
												command.Prepare();
												try {
													var reader = command.ExecuteReader();
													JArray array = new JArray();
													while (reader.Read()) {
														JObject o = new JObject {
															{ "login", JToken.FromObject(reader["logins"]) },
															{ "dc", JToken.FromObject(reader["dc"]) }
														};
														array.Add(o);
													}
													reader.Close();
													EchoJson(new { state = true, invite = array });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "accept": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												string recipient = json["login"].Value<string>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("dialog_accept", connection) {
													CommandType = System.Data.CommandType.StoredProcedure };
												command.Parameters.AddWithValue("@INlogins", _helper.Auth.Login);
												command.Parameters.AddWithValue("@INloginr", recipient);
												command.Parameters.AddWithValue("@INdc", dc);
												command.Prepare();
												try {
													object res = command.ExecuteScalar();
													if ((long)res == 1) {
														JObject o = new JObject {
															{ "type", type },
															{ "act", act },
															{ "login", _helper.Auth.Login }
														};
														var WSact = new WebSocketAct() { Body = JsonConvert.SerializeObject(o) };
														WSact.Recepients.Add(recipient);
														_helper.WShelper.ActsForAll = false;
														_helper.WShelper.Acts.Add(WSact);
														EchoJson(new { state = true });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "list": {
												connection.Open();
												MySqlCommand command = new MySqlCommand($"select d1.loginr, d1.dc dc from dialog d1 join dialog d2 on d1.logins = d2.loginr and d1.loginr = d2.logins where d1.logins = '{_helper.Auth.Login}'", connection);
												try {
													var reader = command.ExecuteReader();
													JArray array = new JArray();
													while (reader.Read()) {
														JObject o = new JObject();
														o.Add("login", JToken.FromObject(reader["loginr"]));
														o.Add("dc", JToken.FromObject(reader["dc"]));
														array.Add(o);
													}
													reader.Close();
													EchoJson(new { state = true, list = array });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "get": {
												int count = json["count"].Value<int>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT logins, id, idh, message, del FROM chat.dialogmessage " +
													$"where (logins=@ls and loginr=@lr) or (logins=@lr and loginr=@ls) order by id*256+idh desc limit {(count > 50 ? 50 : count)}", connection);
												command.Parameters.AddWithValue("@ls", _helper.Auth.Login);
												command.Parameters.AddWithValue("@lr", json["id"].Value<string>());
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													JArray array = new JArray();
													Stack<JObject> tmp = new Stack<JObject>();
													while (reader.Read()) {
														if (!Convert.ToBoolean(reader["del"])) {
															tmp.Push(new JObject {
															{ "logins", JToken.FromObject(reader["logins"]) },
															{ "id", JToken.FromObject(reader["id"]) },
															{ "idh", JToken.FromObject(reader["idh"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														});
														}
													}
													reader.Close();
													while (tmp.Count > 0) {
														array.Add(tmp.Pop());
													}
													EchoJson(new { state = true, list = array });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "getone": {
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT logins, message, del FROM chat.dialogmessage where ((logins=@ls and loginr=@lr) or (logins=@lr and loginr=@ls)) and del = 0 and id=@id and idh=@idh", connection);
												command.Parameters.AddWithValue("@ls", _helper.Auth.Login);
												command.Parameters.AddWithValue("@lr", json["chatid"].Value<string>());
												command.Parameters.AddWithValue("@id", json["id"].Value<string>());
												command.Parameters.AddWithValue("@idh", json["idh"].Value<string>());
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													if (reader.Read()) {
														JObject o = new JObject {
															{ "login", JToken.FromObject(reader["logins"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														};
														EchoJson(new { state = true, o = o });
													}
													else {
														EchoJson(new { state = false, code = 0 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "add": {
												connection.Open();
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												MySqlCommand command = new MySqlCommand("chat.dialogmessage_add", connection) {
													CommandType = System.Data.CommandType.StoredProcedure
												};
												command.Parameters.AddWithValue("INls", _helper.Auth.Login);
												command.Parameters.AddWithValue("INlr", json["id"].Value<string>());
												command.Parameters.AddWithValue("INid", dc);
												command.Parameters.AddWithValue("INm", json["message"].Value<string>());
												command.Parameters.Add("OUTidh", MySqlDbType.UByte);
												command.Parameters["OUTidh"].Direction = System.Data.ParameterDirection.Output;
												try {
													command.Prepare();
													command.ExecuteNonQuery();
													_helper.WShelper.ActsForAll = true;
													JObject o = new JObject {
														{ "type", type },
														{ "act", "add" },
														{ "cid1", _helper.Auth.Login },
														{ "cid2", json["id"].Value<string>() },
														{ "id", dc },
														{ "idh", (byte)command.Parameters["OUTidh"].Value }
													};
													_helper.WShelper.ActsForAll = false;
													var ws = new WebSocketAct() { Body = JsonConvert.SerializeObject(o) };
													ws.Recepients.Add(_helper.Auth.Login);
													ws.Recepients.Add(json["id"].Value<string>());
													_helper.WShelper.Acts.Add(ws);
													EchoJson(new { state = true });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "del": {
												break;
											}
									}
									break;
								}
							case "room": {
									switch (act) {
										case "create": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												connection.Open();
												MySqlCommand command = new MySqlCommand("room_create", connection) { CommandType = System.Data.CommandType.StoredProcedure };
												command.Parameters.AddWithValue("@INid", json["id"].Value<string>());
												command.Parameters.AddWithValue("@INname", json["name"].Value<string>());
												command.Parameters.AddWithValue("@INowner", _helper.Auth.Login);
												command.Parameters.AddWithValue("@INdc", dc);
												command.Prepare();
												try {
													object res = command.ExecuteScalar();
													if ((long)res == 1) {
														EchoJson(new { state = true, dc = dc});
													}
													else {
														EchoJson(new { state = false, code = 2 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "join": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												string id = json["id"].Value<string>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("roomuser_add", connection) { CommandType = System.Data.CommandType.StoredProcedure };
												command.Parameters.AddWithValue("@INid", id);
												command.Parameters.AddWithValue("@INlogin", _helper.Auth.Login);
												command.Parameters.AddWithValue("@INdc", dc);
												command.Parameters.AddWithValue("@Result", dc);
												command.Parameters["@Result"].Direction = System.Data.ParameterDirection.ReturnValue;
												command.Parameters["@Result"].DbType = System.Data.DbType.Int64;
												command.Prepare();
												try {
													command.ExecuteScalar();
													if ((long)command.Parameters["@Result"].Value == 1) {
														command.CommandType = System.Data.CommandType.Text;
														command = new MySqlCommand("select r.* from room r left join roomuser ru on r.id=ru.id_room where r.id= @id ORDER BY r.id desc", connection);
														command.Parameters.Clear();
														command.Parameters.AddWithValue("@id", id);
														command.Prepare();
														var reader = command.ExecuteReader();
														if (reader.Read()) {
															JObject o = new JObject {
																{ "id", JToken.FromObject(reader["id"]) },
																{ "name", JToken.FromObject(reader["name"]) },
																{ "owner", JToken.FromObject(reader["owner"]) },
																{ "dc", JToken.FromObject(reader["dc"]) }
															};
															reader.Close();
															command.CommandText = "select rc.* from room r left join roomuser ru on r.id=ru.id_room left join roomchannel rc on rc.id_room=r.id where r.id=@id ORDER BY rc.id_room, rc.id desc";
															command.Prepare();
															reader = command.ExecuteReader();
															JArray array1 = new JArray();
															while (reader.Read()) {
																JObject o1 = new JObject {
																	{ "idr", JToken.FromObject(reader["id_room"]) },
																	{ "id", JToken.FromObject(reader["id"]) },
																	{ "dc", JToken.FromObject(reader["dc"]) }
																};
																array1.Add(o1);
															}
															reader.Close();
															EchoJson(new { state = true, room = o, listch = array1 });
														}
														else {
															EchoJson(new { state = false, code = 2 });
														}
													}
													else {
														EchoJson(new { state = false, code = 2 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "addch": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												connection.Open();
												MySqlCommand command = new MySqlCommand("roomchannel_create", connection) { CommandType = System.Data.CommandType.StoredProcedure };
												command.Parameters.AddWithValue("@INidr", json["idr"].Value<string>());
												command.Parameters.AddWithValue("@INid", json["idch"].Value<string>());
												command.Parameters.AddWithValue("@INdc", dc);
												command.Prepare();
												try {
													object res = command.ExecuteScalar();
													if ((long)res == 1) {
														EchoJson(new { state = true });
													}
													else {
														EchoJson(new { state = false, code = 2 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "add": {
												ulong dc = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000;
												string cid1 = json["id"].Value<string>();
												string cid2 = json["ch"].Value<string>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("roomchannelmessage_add", connection) {
													CommandType = System.Data.CommandType.StoredProcedure
												};
												command.Parameters.AddWithValue("INidr", cid1);
												command.Parameters.AddWithValue("INidc", cid2);
												command.Parameters.AddWithValue("INl", _helper.Auth.Login);
												command.Parameters.AddWithValue("INid", dc);
												command.Parameters.AddWithValue("INm", json["message"].Value<string>());
												command.Parameters.Add("OUTidh", MySqlDbType.UByte);
												command.Parameters["OUTidh"].Direction = System.Data.ParameterDirection.Output;
												try {
													command.Prepare();
													command.ExecuteNonQuery();
													_helper.WShelper.ActsForAll = true;
													JObject o = new JObject {
														{ "type", type },
														{ "act", "add" },
														{ "cid1", cid1 },
														{ "cid2", cid2 },
														{ "id", dc },
														{ "idh", (byte)command.Parameters["OUTidh"].Value }
													};
													_helper.WShelper.ActsForAll = false;
													var ws = new WebSocketAct() { Body = JsonConvert.SerializeObject(o) };
													command.CommandText = "select login from roomuser where id_room = @idr";
													command.CommandType = System.Data.CommandType.Text;
													command.Parameters.Clear();
													command.Parameters.AddWithValue("@idr", cid1);
													command.Prepare();
													var reader = command.ExecuteReader();
													while (reader.Read()) {
														ws.Recepients.Add(reader["login"].ToString());
													}
													reader.Close();
													_helper.WShelper.Acts.Add(ws);
													EchoJson(new { state = true });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "room": {
												connection.Open();
												string id = json["id"].Value<string>();
												MySqlCommand command = new MySqlCommand("select r.* from room r left join roomuser ru on r.id=ru.id_room where r.id= @id ORDER BY r.id desc", connection);
												command.Parameters.AddWithValue("@id", id);
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													if (reader.Read()) {
														JObject o = new JObject {
															{ "id", JToken.FromObject(reader["id"]) },
															{ "name", JToken.FromObject(reader["name"]) },
															{ "owner", JToken.FromObject(reader["owner"]) },
															{ "dc", JToken.FromObject(reader["dc"]) }
														};
														reader.Close();
														command.CommandText = "select rc.* from room r left join roomuser ru on r.id=ru.id_room left join roomchannel rc on rc.id_room=r.id where r.id=@id ORDER BY rc.id_room, rc.id desc";
														command.Prepare();
														reader = command.ExecuteReader();
														JArray array1 = new JArray();
														while (reader.Read()) {
															JObject o1 = new JObject {
															{ "idr", JToken.FromObject(reader["id_room"]) },
															{ "id", JToken.FromObject(reader["id"]) },
															{ "dc", JToken.FromObject(reader["dc"]) }
														};
															array1.Add(o1);
														}
														reader.Close();
														EchoJson(new { state = true, room = o, listch = array1 });
													}
													else {
														EchoJson(new { state = false, code = 2 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "list": {
												connection.Open();
												MySqlCommand command = new MySqlCommand("select r.* from room r left join roomuser ru on r.id = ru.id_room where ru.login = @login  ORDER BY r.id desc", connection);
												command.Parameters.AddWithValue("@login", _helper.Auth.Login);
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													JArray array1 = new JArray();
													while (reader.Read()) {
														JObject o = new JObject {
															{ "id", JToken.FromObject(reader["id"]) },
															{ "name", JToken.FromObject(reader["name"]) },
															{ "owner", JToken.FromObject(reader["owner"]) },
															{ "dc", JToken.FromObject(reader["dc"]) }
														};
														array1.Add(o);
													}
													reader.Close();
													command.CommandText = "select rc.* from room r left join roomuser ru on r.id = ru.id_room left join roomchannel rc on rc.id_room = r.id where ru.login = @login ORDER BY rc.id_room, rc.id desc;";
													command.Prepare();
													reader = command.ExecuteReader();
													JArray array2 = new JArray();
													while (reader.Read()) {
														JObject o = new JObject {
															{ "idr", JToken.FromObject(reader["id_room"]) },
															{ "id", JToken.FromObject(reader["id"]) },
															{ "dc", JToken.FromObject(reader["dc"]) }
														};
														array2.Add(o);
													}
													reader.Close();
													EchoJson(new { state = true, list = array1, listch = array2 });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "get": {
												int count = json["count"].Value<int>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, id, idh, message, del FROM roomchannelmessage " +
													$"where id_room=@id1 and id_channel=@id2 order by id*256+idh desc limit {(count > 50 ? 50 : count)}", connection);
												command.Parameters.AddWithValue("@id1", json["id"].Value<string>());
												command.Parameters.AddWithValue("@id2", json["idch"].Value<string>());
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													JArray array = new JArray();
													Stack<JObject> tmp = new Stack<JObject>();
													while (reader.Read()) {
														if (!Convert.ToBoolean(reader["del"])) {
															tmp.Push(new JObject {
															{ "login", JToken.FromObject(reader["login"]) },
															{ "id", JToken.FromObject(reader["id"]) },
															{ "idh", JToken.FromObject(reader["idh"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														});
														}
													}
													reader.Close();
													while (tmp.Count > 0) {
														array.Add(tmp.Pop());
													}
													EchoJson(new { state = true, list = array });
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "getone": {
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, message, del FROM roomchannelmessage where id_room=@id1 and id_channel=@id2 and del = 0 and id=@id and idh=@idh", connection);
												command.Parameters.AddWithValue("@id1", json["chatid"].Value<string>());
												command.Parameters.AddWithValue("@id2", json["chatch"].Value<string>());
												command.Parameters.AddWithValue("@id", json["id"].Value<string>());
												command.Parameters.AddWithValue("@idh", json["idh"].Value<string>());
												try {
													command.Prepare();
													var reader = command.ExecuteReader();
													if (reader.Read()) {
														JObject o = new JObject {
															{ "login", JToken.FromObject(reader["login"]) },
															{ "message", JToken.FromObject(reader["message"]) }
														};
														reader.Close();
														EchoJson(new { state = true, o = o });
													}
													else {
														EchoJson(new { state = false, code = 0 });
													}
												}
												catch (Exception e) {
													EchoJson(new { state = false, code = 1 });
												}
												break;
											}
										case "del": {
												break;
											}
									}
									break;
								}
						}
						if(connection.State == System.Data.ConnectionState.Open) {
							connection.Close();
						}
					}
				}
				catch (JsonReaderException e) {
					_helper.Answer(400, "");
				}
				catch (Exception e) {
					_helper.Answer500(e);
				}
			}
			else {
				Echo("<div id=\"chatcontent\" class=\"container-fluid row align-items-end p-0 m-0\" style=\"flex-grow:1;overflow-y:auto;\"></div>");
				Echo("<script src=\"/client/chat.js?1\"></script>");
			}
		}
	}
}
