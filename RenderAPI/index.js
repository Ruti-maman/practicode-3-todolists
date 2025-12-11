const express = require('express');
const fetch = require('node-fetch');

const app = express();
const PORT = process.env.PORT || 3000;

// Render API Key - set as Environment Variable in Render
const RENDER_API_KEY = process.env.RENDER_API_KEY;

// GET endpoint - returns list of services from Render
app.get('/', async (req, res) => {
  try {
    if (!RENDER_API_KEY) {
      return res.status(500).json({ error: 'RENDER_API_KEY is not set' });
    }

    const response = await fetch('https://api.render.com/v1/services?limit=20', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${RENDER_API_KEY}`,
        'Accept': 'application/json'
      }
    });

    if (!response.ok) {
      return res.status(response.status).json({ error: 'Failed to fetch services from Render API' });
    }

    const services = await response.json();
    res.json(services);

  } catch (error) {
    console.error('Error fetching services:', error);
    res.status(500).json({ error: error.message });
  }
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});
