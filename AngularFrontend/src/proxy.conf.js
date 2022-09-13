const PROXY_CONFIG = [
  {
    context: [
      "/cpptester",
    ],
    target: "https://localhost:7024",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
