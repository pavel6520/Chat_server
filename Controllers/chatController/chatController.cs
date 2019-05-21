using ConnectionWorker;
using ConnectionWorker.Helpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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
						var connection = new MySqlConnection(_helper.dbConnectString);
						switch (type) {
							case "public": {
									switch (json["act"].Value<string>()) {
										case "get": {
												int count = json["count"].Value<int>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, timemes, helpid, message, del FROM chat.publicmessage " +
													$"order by timemes, helpid desc limit {(count > 50 ? 50 : count)}", connection);
												var reader = command.ExecuteReader();
												JArray array = new JArray();
												while (reader.Read()) {
													if (!Convert.ToBoolean(reader["del"])) {
														JObject o = new JObject();
														o.Add("login", JToken.FromObject(reader["login"]));
														o.Add("id", JToken.FromObject(reader["timemes"]));
														o.Add("idh", JToken.FromObject(reader["helpid"]));
														o.Add("message", JToken.FromObject(reader["message"]));
														array.Add(o);
													}
												}
												reader.Close();
												EchoJson(array);
												break;
											}
										case "getone": {
												int count = json["count"].Value<int>();
												connection.Open();
												MySqlCommand command = new MySqlCommand("SELECT login, timemes, helpid, message, del FROM chat.publicmessage " +
													$"where timemes='{json["id"].Value<string>()}'", connection);
												var reader = command.ExecuteReader();
												JArray array = new JArray();
												while (reader.Read()) {
													if (!Convert.ToBoolean(reader["del"])) {
														JObject o = new JObject();
														o.Add("login", JToken.FromObject(reader["login"]));
														o.Add("id", JToken.FromObject(reader["timemes"]));
														o.Add("idh", JToken.FromObject(reader["helpid"]));
														o.Add("message", JToken.FromObject(reader["message"]));
														array.Add(o);
													}
												}
												reader.Close();
												EchoJson(array);
												break;
											}
										case "add": {
												connection.Open();
												string timemes = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
												MySqlCommand command = new MySqlCommand("chat.publicmessage_add", connection);
												command.CommandType = System.Data.CommandType.StoredProcedure;
												command.Parameters.AddWithValue("INl", _helper.Auth.Login);
												command.Parameters.AddWithValue("INtm", timemes);
												command.Parameters.AddWithValue("INm", json["message"].Value<string>());
												command.Parameters.Add("OUTh", MySqlDbType.UByte);
												command.Parameters["OUTh"].Direction = System.Data.ParameterDirection.Output;
												try {
													command.Prepare();
													command.ExecuteNonQuery();
													_helper.WShelper.ActsForAll = true;
													JObject o = new JObject();
													o.Add("type", type);
													o.Add("act", "add");
													o.Add("id", timemes);
													o.Add("idh", (byte)command.Parameters["OUTh"].Value);
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
									break;
								}
							case "room": {
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
				Echo("<script src=\"/client/chat.js\"></script>");
			}
		}
	}
}
