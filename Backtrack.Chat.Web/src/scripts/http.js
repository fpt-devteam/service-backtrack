var Http = (() => {

  // Setup request for json
  var getOptions = (verb, data) => {
    var options = {
      dataType: 'json',
      method: verb,
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
    };
    if (!!data) {
      options.body = JSON.stringify(data);
    }
    return options;
  };

  // Handle API response format: { success, data?, error?, correlationId? }
  var handleResponse = async (response) => {
    const result = await response.json();

    // Check if response follows the new API format
    if (result.success === false) {
      // Error response
      const error = new Error(result.error?.message || 'Request failed');
      error.code = result.error?.code || 'UnknownError';
      error.correlationId = result.correlationId;
      error.statusCode = response.status;
      throw error;
    }

    // Success response - return the data
    return result.data !== undefined ? result.data : result;
  };

  // Set Http methods
  return {
    get: (path) => fetch(path, getOptions('GET')).then(handleResponse),
    post: (path, data) => fetch(path, getOptions('POST', data)).then(handleResponse),
    put: (path, data) => fetch(path, getOptions('PUT', data)).then(handleResponse),
    delete: (path) => fetch(path, getOptions('DELETE')).then(handleResponse),
  };
})();
