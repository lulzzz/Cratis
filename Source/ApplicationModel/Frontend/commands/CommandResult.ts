// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Represents the result from executing a {@link ICommand}.
 */
export class CommandResult {
    private _ok: boolean;

    /**
     * Creates an instance of command result.
     * @param {Response} response The HTTP response.
     */
    constructor(response: Response) {
        this._ok = response.ok;
    }

    /**
     * Gets whether execution is successful or not.
     */
    get isSuccess() {
        return this._ok;
    }
}