window.BlazerComponents = window.BlazerComponents || {};
window.BlazerComponents.DataTableInterop = {
    initializeDataTable: function (table) {
        $(table).DataTable({
            'paging': true,
            'lengthChange': true,
            'searching': true,
            'ordering': false,
            'info': true,
            'autoWidth': true
        });
    }
}
window.BlazerComponents.Utils = {
    getAllSelectedOptions: function (select) {
        return $(select).val();
    }
}
