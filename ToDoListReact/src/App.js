import React, { useEffect, useState } from "react";
import service from "./service.js";
import Login from "./Login.js";

function App() {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);
  const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem("token"));

  async function getTodos() {
    try {
      const todos = await service.getTasks();
      setTodos(todos);
    } catch (error) {
      if (error.response?.status === 401) {
        localStorage.removeItem("token");
        setIsLoggedIn(false);
      }
    }
  }

  async function createTodo(e) {
    e.preventDefault();
    await service.addTask(newTodo);
    setNewTodo("");
    await getTodos();
  }

  async function updateCompleted(todo, isComplete) {
    await service.setCompleted(todo.id, isComplete);
    await getTodos();
  }

  async function deleteTodo(id) {
    await service.deleteTask(id);
    await getTodos();
  }

  function handleLogout() {
    console.log("Logout clicked");
    service.logout();
    localStorage.removeItem("token");
    setIsLoggedIn(false);
    setTodos([]);
    console.log("isLoggedIn set to false");
  }

  function handleLoginSuccess() {
    setIsLoggedIn(true);
    getTodos();
  }

  useEffect(() => {
    if (isLoggedIn && todos.length === 0) {
      getTodos();
    }
  }, [isLoggedIn]);

  if (!isLoggedIn) {
    console.log("Rendering Login - isLoggedIn is false");
    return <Login onLoginSuccess={handleLoginSuccess} />;
  }

  console.log("Rendering TodoApp - isLoggedIn is true");
  return (
    <section className="todoapp">
      <header className="header">
        <h1>todos</h1>
        <button
          onClick={() => {
            console.log("Button clicked!");
            handleLogout();
          }}
          style={{
            position: "fixed",
            top: "20px",
            right: "20px",
            padding: "10px 20px",
            backgroundColor: "#f44336",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
            zIndex: 10000,
            fontSize: "16px",
            fontWeight: "bold",
          }}
        >
          התנתק
        </button>
        <form onSubmit={createTodo}>
          <input
            className="new-todo"
            placeholder="Well, let's take on the day"
            value={newTodo}
            onChange={(e) => setNewTodo(e.target.value)}
          />
        </form>
      </header>
      <section className="main" style={{ display: "block" }}>
        <ul className="todo-list">
          {todos.map((todo) => {
            return (
              <li className={todo.isComplete ? "completed" : ""} key={todo.id}>
                <div className="view">
                  <input
                    className="toggle"
                    type="checkbox"
                    defaultChecked={todo.isComplete}
                    onChange={(e) => updateCompleted(todo, e.target.checked)}
                  />
                  <label>{todo.name}</label>
                  <button
                    className="destroy"
                    onClick={() => deleteTodo(todo.id)}
                  ></button>
                </div>
              </li>
            );
          })}
        </ul>
      </section>
    </section>
  );
}

export default App;
