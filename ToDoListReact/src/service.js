import axios from "axios";

// ===================================
// 1. Config Defaults - הגדרת baseURL
// ===================================
 axios.defaults.baseURL = "https://todolist-server-psgx.onrender.com";
// axios.defaults.baseURL = "http://localhost:5005";

// ===================================
// פונקציה לשמירת והחזרת token
// ===================================
export const setAuthToken = (token) => {
  if (token) {
    localStorage.setItem("token", token);
    axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  } else {
    localStorage.removeItem("token");
    delete axios.defaults.headers.common["Authorization"];
  }
};

// טעינת token מה-localStorage בעת הרצה ראשונית
const savedToken = localStorage.getItem("token");

if (savedToken) {
  axios.defaults.headers.common["Authorization"] = `Bearer ${savedToken}`;
}

// ===================================
// 2. Interceptor - תפיסת שגיאות
// ===================================
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("❌ Error API:", error.response?.data || error.message);

    // אם קיבלנו 401 (לא מורשה), מעבירים ללוגין
    if (error.response && error.response.status === 401) {
      localStorage.removeItem("token");
      delete axios.defaults.headers.common["Authorization"];
      window.location.href = "/login";
    }

    return Promise.reject(error);
  }
);

// ===================================
// API Functions
// ===================================
const service = {
  getTasks: async () => {
    const result = await axios.get("/tasks");
    return result.data;
  },

  addTask: async (name) => {
    console.log("addTask", name);
    const result = await axios.post("/tasks", {
      name,
      isComplete: false,
    });
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    console.log("setCompleted", { id, isComplete });
    // שלוף את המשימה הקיימת
    const task = await axios.get(`/tasks/${id}`);
    // עדכן עם כל הנתונים
    await axios.put(`/tasks/${id}`, {
      name: task.data.name,
      isComplete,
    });
    return {};
  },

  deleteTask: async (id) => {
    console.log("deleteTask", id);
    await axios.delete(`/tasks/${id}`);
  },

  // ===================================
  // JWT Auth Functions
  // ===================================
  register: async (userName, password) => {
    const result = await axios.post("/auth/register", { userName, password });
    return result.data;
  },

  login: async (userName, password) => {
    const result = await axios.post("/auth/login", { userName, password });
    const token = result.data.token;
    setAuthToken(token);
    return token;
  },

  logout: () => {
    setAuthToken(null);
  },
};

export default service;
