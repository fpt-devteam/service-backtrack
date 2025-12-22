import path from 'path';
import dotenv from 'dotenv';
import moduleAlias from 'module-alias';


// Configure "dotenv" - load from .env file in root
dotenv.config({
  path: path.join(__dirname, '.env'),
});

// Configure moduleAlias
if (__filename.endsWith('js')) {
  moduleAlias.addAlias('@src', __dirname + '/dist');
}
