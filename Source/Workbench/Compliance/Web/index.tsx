// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import 'reflect-metadata';
import ReactDOM from 'react-dom';

import './index.scss';
import '@cratis/workbench.styles/theme';

import { App } from './App';
import { ModalProvider } from '@cratis/fluentui';

ReactDOM.render(
    <ModalProvider>
        <App />
    </ModalProvider>,
    document.getElementById('root')
);