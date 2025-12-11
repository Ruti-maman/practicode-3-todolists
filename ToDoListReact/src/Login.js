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
    <div className="login-container">
      <h2>{isRegistering ? "הרשמה" : "התחברות"}</h2>
      <form onSubmit={handleSubmit} style={{ width: "100%" }}>
        <input
          type="text"
          placeholder="שם משתמש"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="סיסמה"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit">{isRegistering ? "הירשם" : "התחבר"}</button>
      </form>
      {error && <div className="login-error">{error}</div>}
      <button
        className="login-toggle"
        onClick={() => {
          setIsRegistering(!isRegistering);
          setError("");
        }}
      >
        {isRegistering ? "כבר יש לי חשבון - התחבר" : "אין לי חשבון - הירשם"}
      </button>
    </div>
  );
}
