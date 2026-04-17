const fs = require('node:fs');
const path = require('node:path');

const targetPath = path.join(__dirname, '..', 'public', 'runtime-config.js');
const apiUrl = process.env.ASPIRE_GATEWAY_URL?.trim();

const runtimeConfig = apiUrl ? { apiUrl } : {};
const content = `window.__ATTENDR_RUNTIME_CONFIG__ = ${JSON.stringify(runtimeConfig, null, 2)};\n`;

fs.writeFileSync(targetPath, content, 'utf8');
console.log(`Generated runtime-config.js${apiUrl ? ` with API URL ${apiUrl}` : ' with fallback defaults'}.`);
