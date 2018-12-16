window.BlazorComponents = window.BlazorComponents || {};
window.BlazorComponents.DataTableInterop = {
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
};
window.BlazorComponents.Utils = {
    getAllSelectedOptions: function (select) {
        return $(select).val();
    },
    failValidation: function (id) {
        console.log('Set invalid for ' + id);
        $('#' + id).children(':input').addClass('is-invalid', 'was-validated');
    },
    succeedValidation: function (id) {
        $('#' + id).children(':input').removeClass('is-invalid', 'was-validated');
    },
    buttonState: function (btn, state) {
        var button = $(btn);
        if (state === 'loading') {
            var loadingText = button.data('loading-text');
            button.html(loadingText);
            button.attr('disabled', true);
        } else {
            var loadingText = button.data('normal-text');
            button.html(loadingText);
            button.removeAttr('disabled');
        }
    },
    print: function () {
        window.print();
    }
};

window.BlazorComponents.DateRangePicker = {
    init: function (element, blazorComponent) {
        var start = moment().subtract(6, 'days').format('MM/DD/YYYY');
        var end = moment().format('MM/DD/YYYY');
        $(element).daterangepicker({
            "autoApply": true,
            ranges: {
                'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                'Last Quater': [moment().subtract(3, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
            },
            "alwaysShowCalendars": true,
            "autoUpdateInput": true,
            "startDate": start,
            "endDate": end,
            "minDate": moment().subtract(6, 'month'),
            "maxDate": moment().add(1, 'days'),
            "opens": "right"
        })
        .on('apply.daterangepicker', function (ev, picker) {
            blazorComponent.invokeMethod("OnRangeChanged", picker.startDate.format('YYYY-MM-DD'), picker.endDate.format('YYYY-MM-DD'))
            });
        blazorComponent.invokeMethod("OnRangeChanged", start, end);
    },

    getStartDate: function (element) {
        return $(element).data('daterangepicker').startDate.format('MM/DD/YYYY');
    },

    getEndDate: function (element) {
        return $(element).data('daterangepicker').endDate.format('MM/DD/YYYY');
    }
};
