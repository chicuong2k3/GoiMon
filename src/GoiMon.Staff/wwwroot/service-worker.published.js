self.assetsInclude = [/\.dll$/, /\.wasm$/, /\.js$/, /\.json$/, /\.css$/, /\.html$/, /\.dat$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.svg$/, /\.ico$/, /\.woff2?$/, /\.ttf$/, /\.webp$/];
self.assetsExclude = [/^_content\/Bit\.Bswup\/bit-bswup\.sw\.js$/, /^service-worker\.js$/];
self.defaultUrl = '/';
self.serverHandledUrls = [/\/api\//, /\/graphql$/];
self.assetsUrl = '/service-worker-assets.js';
self.caseInsensitiveUrl = true;
self.enableIntegrityCheck = true;

self.importScripts('_content/Bit.Bswup/bit-bswup.sw.js');
