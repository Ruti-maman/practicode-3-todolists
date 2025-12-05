import React, { useState } from "react";
import service from "./service";

export default function Login({ onLoginSuccess }) {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isRegistering, setIsRegistering] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    try {
      if (isRegistering) {
        // הרשמה
        await service.register(userName, password);
        // לאחר הרשמה מוצלחת, לוגין אוטומטי
        await service.login(userName, password);
        if (onLoginSuccess) {
          onLoginSuccess();
        } else {
          window.location.href = "/";
        }
      } else {
        // התחברות רגילה
        await service.login(userName, password);
        if (onLoginSuccess) {
          onLoginSuccess();
        } else {
          window.location.href = "/";
        }
      }
    } catch (err) {
      if (isRegistering) {
        setError(err.response?.data || "שגיאה בהרשמה");
      } else {
        setError("שם משתמש או סיסמה שגויים");
      }
    }
  };

  return (
    <div
      style={{
        maxWidth: "400px",
        margin: "50px auto",
        padding: "20px",
        border: "1px solid #ccc",
        borderRadius: "8px",
      }}
    >
      <h2>{isRegistering ? "הרשמה" : "התחברות"}</h2>
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: "15px" }}>
          <input
            style={{ width: "100%", padding: "10px", fontSize: "16px" }}
            placeholder="שם משתמש"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            required
          />
        </div>
        <div style={{ marginBottom: "15px" }}>
          <input
            style={{ width: "100%", padding: "10px", fontSize: "16px" }}
            placeholder="סיסמה"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button
          type="submit"
          style={{
            width: "100%",
            padding: "10px",
            fontSize: "16px",
            backgroundColor: "#4CAF50",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
          }}
        >
          {isRegistering ? "הירשם" : "התחבר"}
        </button>
      </form>

      {error && <div style={{ color: "red", marginTop: "10px" }}>{error}</div>}

      <div style={{ marginTop: "15px", textAlign: "center" }}>
        <button
          onClick={() => {
            setIsRegistering(!isRegistering);
            setError("");
          }}
          style={{
            background: "none",
            border: "none",
            color: "#2196F3",
            cursor: "pointer",
            textDecoration: "underline",
          }}
        >
          {isRegistering ? "כבר יש לי חשבון - התחבר" : "אין לי חשבון - הירשם"}
        </button>
      </div>
    </div>
  );
}
